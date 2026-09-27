import type { ArtworkSummary } from './api'
import { throwIfNotOk } from './http'

export interface CollectionSummary {
  id: number
  name: string
  artworkCount: number
  createdAt: string
}

export interface CollectionDetail {
  id: number
  name: string
  createdAt: string
  artworks: ArtworkSummary[]
}

function authHeaders(token: string): HeadersInit {
  return { Authorization: `Bearer ${token}` }
}

export async function listCollections(token: string): Promise<CollectionSummary[]> {
  const response = await fetch('/api/collections', { headers: authHeaders(token) })
  await throwIfNotOk(response)
  return (await response.json()) as CollectionSummary[]
}

export async function createCollection(token: string, name: string): Promise<CollectionSummary> {
  const response = await fetch('/api/collections', {
    method: 'POST',
    headers: { ...authHeaders(token), 'Content-Type': 'application/json' },
    body: JSON.stringify({ name }),
  })
  await throwIfNotOk(response)
  return (await response.json()) as CollectionSummary
}

export async function renameCollection(token: string, id: number, name: string): Promise<CollectionSummary> {
  const response = await fetch(`/api/collections/${id}`, {
    method: 'PUT',
    headers: { ...authHeaders(token), 'Content-Type': 'application/json' },
    body: JSON.stringify({ name }),
  })
  await throwIfNotOk(response)
  return (await response.json()) as CollectionSummary
}

export async function deleteCollection(token: string, id: number): Promise<void> {
  const response = await fetch(`/api/collections/${id}`, { method: 'DELETE', headers: authHeaders(token) })
  await throwIfNotOk(response)
}

export async function getCollection(token: string, id: number): Promise<CollectionDetail> {
  const response = await fetch(`/api/collections/${id}`, { headers: authHeaders(token) })
  await throwIfNotOk(response)
  return (await response.json()) as CollectionDetail
}

export async function addArtworkToCollection(token: string, collectionId: number, artworkId: number): Promise<void> {
  const response = await fetch(`/api/collections/${collectionId}/artworks/${artworkId}`, {
    method: 'POST',
    headers: authHeaders(token),
  })
  await throwIfNotOk(response)
}

export async function removeArtworkFromCollection(
  token: string,
  collectionId: number,
  artworkId: number,
): Promise<void> {
  const response = await fetch(`/api/collections/${collectionId}/artworks/${artworkId}`, {
    method: 'DELETE',
    headers: authHeaders(token),
  })
  await throwIfNotOk(response)
}
