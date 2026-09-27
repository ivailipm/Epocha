export interface AuthSession {
  token: string
  expiresAt: string
  userId: number
  email: string
  displayName: string
}

async function postAuth(path: string, body: unknown): Promise<AuthSession> {
  const response = await fetch(path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response))
  }

  return (await response.json()) as AuthSession
}

// The API returns either { detail: "..." } (a hand-written Problem response) or ASP.NET Core's
// automatic { errors: { Field: ["message"] } } shape for validation failures.
async function extractErrorMessage(response: Response): Promise<string> {
  const problem = await response.json().catch(() => null)

  if (typeof problem?.detail === 'string') {
    return problem.detail
  }

  if (problem?.errors && typeof problem.errors === 'object') {
    const messages = Object.values(problem.errors).flat()
    if (messages.length > 0) {
      return messages.join(' ')
    }
  }

  return 'Something went wrong. Please try again.'
}

export const register = (email: string, password: string, displayName: string) =>
  postAuth('/api/auth/register', { email, password, displayName })

export const login = (email: string, password: string) => postAuth('/api/auth/login', { email, password })
