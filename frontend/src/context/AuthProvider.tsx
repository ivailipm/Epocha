import { useState, type ReactNode } from 'react'
import * as auth from '../lib/auth'
import type { AuthSession } from '../lib/auth'
import { AuthContext, type AuthContextValue } from './AuthContext'

const STORAGE_KEY = 'epocha.session'

function loadSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as AuthSession) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(loadSession)

  function persist(next: AuthSession | null) {
    setSession(next)
    try {
      if (next) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
      } else {
        localStorage.removeItem(STORAGE_KEY)
      }
    } catch {
      // Private browsing etc. can throw; the session still works for this tab via state.
    }
  }

  const value: AuthContextValue = {
    session,
    login: async (email, password) => persist(await auth.login(email, password)),
    register: async (email, password, displayName) => persist(await auth.register(email, password, displayName)),
    logout: () => persist(null),
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
