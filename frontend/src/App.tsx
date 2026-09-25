import { Link, Route, Routes } from 'react-router'
import { ArtworkDetailPage } from './components/ArtworkDetailPage'
import { Gallery } from './components/Gallery'

function App() {
  return (
    <main>
      <h1>
        <Link to="/">Epocha</Link>
      </h1>
      <Routes>
        <Route path="/" element={<Gallery />} />
        <Route path="/artworks/:id" element={<ArtworkDetailPage />} />
      </Routes>
    </main>
  )
}

export default App
