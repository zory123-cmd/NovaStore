import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4">
      <div className="text-center">
        <h1 className="text-8xl font-bold text-primary-500 mb-4">404</h1>
        <h2 className="text-xl font-medium text-gray-900 mb-2">Страница не найдена</h2>
        <p className="text-gray-500 mb-6">Извините, такой страницы не существует</p>
        <Link
          to="/"
          className="inline-block px-6 py-3 bg-gradient-to-r from-primary-500 to-blue-500 text-white rounded-lg font-medium text-sm hover:from-primary-600 hover:to-blue-600 transition-all shadow-md shadow-primary-200"
        >
          На главную
        </Link>
      </div>
    </div>
  )
}
