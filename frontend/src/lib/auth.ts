import { throwIfNotOk } from './http'

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

  await throwIfNotOk(response)
  return (await response.json()) as AuthSession
}

export const register = (email: string, password: string, displayName: string) =>
  postAuth('/api/auth/register', { email, password, displayName })

export const login = (email: string, password: string) => postAuth('/api/auth/login', { email, password })
