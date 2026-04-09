import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { VerifyOtp } from './features/auth/verify-otp/verify-otp';
import { ResetPassword } from './features/auth/reset-password/reset-password';
import { Dashboard } from './features/dashboard/dashboard';
import { Profile } from './features/profile/profile';
import { ProfilePictureUpload } from './features/profile/profile-picture-upload/profile-picture-upload';
import { CardList } from './features/cards/card-list/card-list';
import { AddCard } from './features/cards/add-card/add-card';
import { CardDetails } from './features/cards/card-details/card-details';
import { Payments } from './features/payments/payments';
import { Transactions } from './features/transactions/transactions';
import { Notifications } from './features/notifications/notifications';
import { Rewards } from './features/rewards/rewards';
import { Support } from './features/support/support';
import { Landing } from './features/landing/landing';
import { About } from './features/about/about';
import { Contact } from './features/contact/contact';
import { AdminCardsComponent } from './features/admin/admin-cards/admin-cards';
import { AdminSupport } from './features/admin/admin-support/admin-support';
import { authGuard } from './core/guards/auth-guard';
import { guestGuard } from './core/guards/guest-guard';

export const routes: Routes = [
  { path: '', component: Landing, pathMatch: 'full' },
  { path: 'about', component: About },
  { path: 'contact', component: Contact },
  
  // Auth Routes (only accessible when NOT logged in)
  { path: 'auth/login', component: Login, canActivate: [guestGuard] },
  { path: 'auth/register', component: Register, canActivate: [guestGuard] },
  { path: 'auth/verify-otp', component: VerifyOtp, canActivate: [guestGuard] },
  { path: 'auth/reset-password', component: ResetPassword, canActivate: [guestGuard] },
  
  // Protected Routes (only accessible when logged in)
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: 'profile', component: Profile, canActivate: [authGuard] },
  { path: 'profile/picture', component: ProfilePictureUpload, canActivate: [authGuard] },
  { path: 'cards', component: CardList, canActivate: [authGuard] },
  { path: 'cards/add', component: AddCard, canActivate: [authGuard] },
  { path: 'cards/:id', component: CardDetails, canActivate: [authGuard] },
  { path: 'payments', component: Payments, canActivate: [authGuard] },
  { path: 'transactions', component: Transactions, canActivate: [authGuard] },
  { path: 'notifications', component: Notifications, canActivate: [authGuard] },
  { path: 'rewards', component: Rewards, canActivate: [authGuard] },
  { path: 'support', component: Support, canActivate: [authGuard] },
  
  // Admin Routes
  { path: 'admin/cards', component: AdminCardsComponent, canActivate: [authGuard] },
  { path: 'admin/support', component: AdminSupport, canActivate: [authGuard] },
  { path: 'admin/users', component: AdminCardsComponent, canActivate: [authGuard] },
  { path: 'admin/transactions', component: AdminCardsComponent, canActivate: [authGuard] },
  { path: 'admin/notifications', component: AdminCardsComponent, canActivate: [authGuard] },
  
  // Fallback - redirect to landing if not authenticated, dashboard if authenticated
  { path: '**', redirectTo: '' }
];
