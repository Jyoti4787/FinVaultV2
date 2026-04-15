export interface ApiResponse<T> {
  success?: boolean;
  message?: string;
  data: T;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  avatarBase64?: string;
}

export interface Card {
  cardId: string;
  maskedNumber: string;
  fullNumber?: string; // Revealed full card number
  cvv?: string; // Revealed CVV
  cardNumberMasked?: string; // Alias for maskedNumber
  cardholderName: string;
  issuerName: string;
  network: string;
  cardType?: string;
  status?: string;
  expiryMonth: number;
  expiryYear: number;
  creditLimit: number;
  outstandingBalance: number;
  availableCredit?: number;
  utilizationPercent: number;
  billingCycleStartDay: number;
  dueDate?: string;
  isDefault: boolean;
  isVerified: boolean;
}

export interface Transaction {
  id: string;
  userId: string;
  amount: number;
  type: string;
  description: string;
  category: string;
  referenceId: string;
  timestamp: string;
  status?: string;
}

export interface AuthResponse {
  message: string;
  userId: string;
  email: string;
  role: string;
  accessToken: string;
  refreshToken: string;
  otpRequired?: boolean;
  requiresOtp?: boolean;
}

export interface Notification {
  id: string;
  message: string;
  type: string;
  date: string;
  isRead: boolean;
  actionUrl?: string | null;
}

export interface Reward {
  id: string;
  userId: string;
  points: number;
  description: string;
  createdAt: string;
}

export interface SupportTicket {
  id: string;
  userId: string;
  subject: string;
  message: string;
  status: string;
  adminComment?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Payment {
  id: string;
  userId: string;
  cardId: string;
  statementId: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
}

export interface CardUtilization {
  cardId: string;
  creditLimit: number;
  outstandingBalance: number;
  utilizationPercent: number;
}

export interface AddCardRequest {
  cardNumber: string;
  cardholderName: string;
  expiryMonth: number;
  expiryYear: number;
  creditLimit: number;
  billingCycleStartDay: number;
  correlationId?: string;
}

export interface ProcessPaymentRequest {
  cardId: string;
  statementId: string;
  amount: number;
  otpCode: string;
  currency?: string;
  correlationId?: string;
  transactionId?: string;
}

export interface RedeemRewardRequest {
  points: number;
  cardId: string;
  amount?: number;
}

export interface RewardSummary {
  points: number;
  history: Reward[];
}

export interface CreateTicketRequest {
  subject: string;
  message: string;
}

export interface PayExternalBillRequest {
  billType: string;
  amount: number;
  accountNumber: string;
  cardId: string;
}

export interface VerifyExternalBillRequest {
  otpCode: string;
}
