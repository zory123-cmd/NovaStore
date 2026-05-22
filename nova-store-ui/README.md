# NovaStore UI — фронтенд маркетплейса

Клиентская часть интернет-магазина **NovaStore** на React + TypeScript + Vite + Tailwind CSS.

---

## Стек

- **React 19** с функциональными компонентами и хуками
- **TypeScript 5.8** — строгая типизация
- **Vite 6** — быстрая сборка и HMR
- **Tailwind CSS 3** — утилитарная стилизация
- **Axios** — HTTP-клиент с перехватчиками (JWT, 401 → редирект на login)
- **React Router DOM 7** — клиентская маршрутизация
- **react-hot-toast** — уведомления

---

## Установка и запуск

### Требования
- Node.js 18+
- npm

### Разработка

```bash
# Установить зависимости
npm install

# Запустить dev-сервер (проксирует /api → http://localhost:5000)
npm run dev
```

Фронтенд будет доступен на **http://localhost:5173**.

Vite настроен на проксирование всех запросов `/api` на `http://localhost:5000` (см. `vite.config.ts`).

### Production-сборка

```bash
npm run build     # Сборка в папку dist/
npm run preview   # Предпросмотр собранного приложения
```

---

## Структура

```
nova-store-ui/
├── index.html                  # Входная точка
├── package.json
├── vite.config.ts              # Конфигурация Vite + proxy на API
├── tailwind.config.js          # Кастомные цвета (primary: indigo)
├── postcss.config.js
├── tsconfig.json
│
└── src/
    ├── main.tsx                # Точка входа React
    ├── App.tsx                 # Роуты и провайдеры
    ├── index.css               # Tailwind directives
    │
    ├── pages/                  # Страницы приложения
    │   ├── Catalog.tsx         # Каталог товаров (главная)
    │   ├── ProductDetail.tsx   # Карточка товара
    │   ├── Cart.tsx            # Корзина
    │   ├── Checkout.tsx        # Оформление заказа
    │   ├── Login.tsx           # Вход
    │   ├── Register.tsx        # Регистрация
    │   ├── MyOrders.tsx        # Мои заказы
    │   ├── OrderDetail.tsx     # Детали заказа
    │   ├── Profile.tsx         # Профиль / адреса
    │   ├── AdminDashboard.tsx  # Админ-панель
    │   └── NotFound.tsx        # 404
    │
    ├── components/             # Переиспользуемые компоненты
    │   ├── Layout.tsx          # Общий layout
    │   ├── Navbar.tsx          # Навигация
    │   ├── Sidebar.tsx         # Боковое меню (категории)
    │   ├── ProductCard.tsx     # Карточка товара в списке
    │   ├── LoadingSpinner.tsx  # Спиннер загрузки
    │   └── Skeleton.tsx        # Скелетон-загрузка
    │
    ├── services/               # API-клиенты
    │   ├── api.ts              # Axios instance (baseURL, JWT, перехватчики)
    │   ├── authService.ts      # /api/auth
    │   ├── productService.ts   # /api/products
    │   ├── categoryService.ts  # /api/categories
    │   ├── cartService.ts      # /api/cart
    │   ├── orderService.ts     # /api/orders
    │   ├── reviewService.ts    # /api/reviews
    │   └── userService.ts      # /api/users
    │
    ├── contexts/
    │   └── AuthContext.tsx      # Контекст аутентификации (token, user, login, logout)
    │
    └── types/
        └── index.ts            # TypeScript интерфейсы (Product, Order, CartItem, и т.д.)
```

---

## API-прокси

В режиме разработки все запросы к `/api/*` проксируются на `http://localhost:5000` (конфигурация в `vite.config.ts`). Это позволяет не настраивать CORS для development.

```ts
// vite.config.ts
server: {
  port: 5173,
  proxy: {
    '/api': {
      target: 'http://localhost:5000',
      changeOrigin: true,
    },
  },
}
```

---

## Аутентификация

JWT-токен сохраняется в `localStorage` и автоматически подставляется в заголовок `Authorization: Bearer <token>` через Axios-перехватчик. При ответе 401 токен очищается, пользователь перенаправляется на `/login`.

---

## Команды

```bash
npm run dev       # Запуск dev-сервера
npm run build     # Production-сборка
npm run preview   # Предпросмотр сборки
```
