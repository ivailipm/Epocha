import { createContext } from 'react'
import type { AuthSession } from '../lib/auth'

export interface AuthContextValue {
  session: AuthSession | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, displayName: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
