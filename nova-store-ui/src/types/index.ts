export interface Product {
  id: number
  name: string
  description?: string
  price: number
  stockQuantity: number
  categoryId: number
  categoryName?: string
  imageUrl?: string
  brand?: string
  isActive: boolean
  averageRating?: number
  reviewsCount: number
  createdAt: string
  updatedAt: string
}

export interface CreateProductDto {
  name: string
  description?: string
  price: number
  stockQuantity?: number
  categoryId: number
  imageUrl?: string
  brand?: string
}

export interface UpdateProductDto {
  name?: string
  description?: string
  price?: number
  stockQuantity?: number
  categoryId?: number
  imageUrl?: string
  brand?: string
  isActive?: boolean
}

export interface ProductSearchParams {
  searchTerm?: string
  categoryId?: number
  minPrice?: number
  maxPrice?: number
  brand?: string
  sortBy?: string
  sortDescending?: boolean
  page?: number
  pageSize?: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface Category {
  id: number
  name: string
  description?: string
  imageUrl?: string
  parentCategoryId?: number
  parentCategoryName?: string
  createdAt: string
  subCategories: Category[]
}

export interface CreateCategoryDto {
  name: string
  description?: string
  imageUrl?: string
  parentCategoryId?: number
}

export interface CartItem {
  id: number
  userId: number
  productId: number
  productName?: string
  productImageUrl?: string
  unitPrice: number
  quantity: number
  subtotal: number
  createdAt: string
}

export interface AddToCartDto {
  productId: number
  quantity: number
}

export interface Order {
  id: number
  userId: number
  customerName: string
  shippingAddressId?: number
  shippingNotes?: string
  paymentMethod: string
  paymentStatus: string
  shippingStatus: string
  totalAmount: number
  discountAmount?: number
  promoCode?: string
  orderDate: string
  shippedAt?: string
  deliveredAt?: string
  status: string
  orderItems: OrderItem[]
}

export interface CreateOrderDto {
  shippingAddressId?: number
  shippingNotes?: string
  paymentMethod: string
  promoCode?: string
}

export interface OrderItem {
  id: number
  orderId: number
  productId: number
  productName: string
  productImageUrl?: string
  unitPrice: number
  quantity: number
  subtotal: number
}

export interface Review {
  id: number
  productId: number
  userId: number
  userName?: string
  rating: number
  comment?: string
  createdAt: string
  updatedAt: string
}

export interface CreateReviewDto {
  productId: number
  rating: number
  comment?: string
}

export interface User {
  id: number
  username: string
  email: string
  fullName?: string
  phone?: string
  avatarUrl?: string
  role: string
  createdAt: string
  lastLoginAt?: string
}

export interface LoginDto {
  username: string
  password: string
}

export interface RegisterUserDto {
  username: string
  email: string
  password: string
  fullName?: string
  phone?: string
}

export interface LoginResponse {
  token: string
  user: User
}

export interface UpdateUserDto {
  fullName?: string
  phone?: string
  avatarUrl?: string
}

export interface ChangePasswordDto {
  currentPassword: string
  newPassword: string
}

export interface Address {
  id: number
  userId: number
  fullName: string
  phone: string
  street: string
  city: string
  state?: string
  zipCode: string
  country: string
  isDefault: boolean
  createdAt: string
}

export interface CreateAddressDto {
  fullName: string
  phone: string
  street: string
  city: string
  state?: string
  zipCode: string
  country: string
  isDefault: boolean
}
