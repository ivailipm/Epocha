import { Link, Route, Routes } from 'react-router'
import { ArtworkDetailPage } from './components/ArtworkDetailPage'
import { CollectionDetailPage } from './components/CollectionDetailPage'
import { CollectionsPage } from './components/CollectionsPage'
import { Gallery } from './components/Gallery'
import { LoginPage } from './components/LoginPage'
import { RegisterPage } from './components/RegisterPage'
import { RequireAuth } from './components/RequireAuth'
import { ThemeToggle } from './components/ThemeToggle'
import { useAuth } from './context/useAuth'

function App() {
  const { session, logout } = useAuth()

  return (
    <main>
      <header className="masthead">
        <div className="masthead-top">
          <Link to="/" className="masthead-wordmark">
            Epocha
          </Link>
          <nav className="auth-nav">
            {session ? (
              <>
                <Link to="/collections">My Collections</Link>
                <span>{session.displayName}</span>
                <button type="button" onClick={logout}>
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link to="/login">Log in</Link>
                <Link to="/register">Register</Link>
              </>
            )}
            <ThemeToggle />
          </nav>
        </div>
        <p className="masthead-tagline">An Art History Timeline Explorer</p>
      </header>

      <Routes>
        <Route path="/" element={<Gallery />} />
        <Route path="/artworks/:id" element={<ArtworkDetailPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/collections"
          element={
            <RequireAuth>
              <CollectionsPage />
            </RequireAuth>
          }
        />
        <Route
          path="/collections/:id"
          element={
            <RequireAuth>
              <CollectionDetailPage />
            </RequireAuth>
          }
        />
      </Routes>
    </main>
  )
}

export default App
