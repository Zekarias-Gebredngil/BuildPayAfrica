import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell.component';
import { HomeComponent } from './features/public/home.component';
import { AboutComponent } from './features/public/about.component';
import { ForbiddenComponent } from './features/public/forbidden.component';
import { MarketplaceComponent } from './features/marketplace/marketplace.component';
import { CartComponent } from './features/marketplace/cart.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { ProjectsComponent } from './features/projects/projects.component';
import { ProjectDetailComponent } from './features/projects/project-detail.component';
import { OrdersComponent } from './features/orders/orders.component';
import { NotificationsComponent } from './features/notifications/notifications.component';
import { AdminComponent } from './features/admin/admin.component';
import { ContractorVerificationComponent } from './features/contractor/contractor-verification.component';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      // ── Public ──────────────────────────────────────────────────────
      { path: '',      pathMatch: 'full', component: HomeComponent },
      { path: 'about',     component: AboutComponent },
      { path: 'forbidden', component: ForbiddenComponent },

      // ── Auth (redirect to home if already logged in) ─────────────────
      { path: 'login',    component: LoginComponent,    canActivate: [guestGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },

      // ── Marketplace (browse public, cart requires auth) ──────────────
      { path: 'marketplace', component: MarketplaceComponent },
      { path: 'cart',        component: CartComponent, canActivate: [authGuard] },

      // ── Authenticated ────────────────────────────────────────────────
      { path: 'projects',     component: ProjectsComponent,      canActivate: [authGuard] },
      { path: 'projects/:id', component: ProjectDetailComponent, canActivate: [authGuard] },
      { path: 'orders',       component: OrdersComponent,        canActivate: [authGuard] },
      { path: 'notifications',component: NotificationsComponent, canActivate: [authGuard] },

      // ── Role-restricted ──────────────────────────────────────────────
      {
        path: 'admin',
        component: AdminComponent,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Admin'] }
      },
      {
        path: 'contractor/verification',
        component: ContractorVerificationComponent,
        canActivate: [authGuard, roleGuard],
        data: { roles: ['Contractor', 'Admin'] }
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
