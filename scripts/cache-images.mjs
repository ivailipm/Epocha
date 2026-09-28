// One-time (re-runnable) tool: downloads every artwork's thumbnail and full image from the
// Art Institute of Chicago and saves them locally as `{id}-thumbnail.jpg` / `{id}-image.jpg`.
//
// Why this exists: the production server runs on a datacenter IP (Hetzner), and the museum's
// image host (www.artic.edu) puts an interactive Cloudflare challenge in front of any request
// from IPs like that — no server-side HTTP client can solve it, so the API can never fetch
// these images live in production (see ArtworkImageService's doc comment). Run this from an
// unflagged network instead (your own machine, on your home/office connection), then copy the
// output into the server's `image-cache` volume:
//
//   node scripts/cache-images.mjs [siteUrl] [outDir]
//   docker cp ./image-cache/. <api-container-name>:/data/images/
//
// Defaults: siteUrl = https://epocha.ivaylapernikova.com, outDir = ./image-cache
// Re-run any time after ingesting new artworks; already-downloaded files are skipped.

const siteUrl = (process.argv[2] ?? 'https://epocha.ivaylapernikova.com').replace(/\/$/, '')
const outDir = process.argv[3] ?? './image-cache'

const REFERER = 'https://www.artic.edu/'
const USER_AGENT = 'Epocha/1.0 (portfolio project)'
const PAGE_SIZE = 100
const DELAY_MS = 150

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms))

async function fetchJson(url) {
  const response = await fetch(url)
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText} for ${url}`)
  }
  return response.json()
}

async function collectArtworkIds() {
  const ids = []
  let page = 1
  for (;;) {
    const result = await fetchJson(`${siteUrl}/api/artworks/search?page=${page}&pageSize=${PAGE_SIZE}`)
    for (const item of result.items) ids.push(item.id)
    if (page >= result.totalPages) break
    page += 1
  }
  return ids
}

async function saveImage(url, destPath) {
  const response = await fetch(url, {
    headers: { Referer: REFERER, 'User-Agent': USER_AGENT },
  })
  if (!response.ok) {
    console.warn(`  skip (${response.status}): ${url}`)
    return
  }
  const bytes = Buffer.from(await response.arrayBuffer())
  await import('node:fs/promises').then((fs) => fs.writeFile(destPath, bytes))
}

async function main() {
  const fs = await import('node:fs/promises')
  const path = await import('node:path')
  await fs.mkdir(outDir, { recursive: true })

  console.log(`Fetching artwork list from ${siteUrl} ...`)
  const ids = await collectArtworkIds()
  console.log(`Found ${ids.length} artworks.`)

  for (const [index, id] of ids.entries()) {
    const detail = await fetchJson(`${siteUrl}/api/artworks/${id}`)
    console.log(`[${index + 1}/${ids.length}] #${id} ${detail.title}`)

    for (const [kind, url] of [
      ['thumbnail', detail.thumbnailUrl],
      ['image', detail.imageUrl],
    ]) {
      if (!url) continue
      const dest = path.join(outDir, `${id}-${kind}.jpg`)
      if (await fs.stat(dest).then(() => true).catch(() => false)) continue
      await saveImage(url, dest)
      await sleep(DELAY_MS)
    }
  }

  console.log(`Done. Files written to ${outDir}`)
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
