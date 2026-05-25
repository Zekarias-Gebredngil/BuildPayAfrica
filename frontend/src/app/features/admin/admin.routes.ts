import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guard';
import { roleGuard } from '../../core/auth/role.guard';
import { AdminDashboardComponent } from './dashboard.component';
import { AdminVerificationsComponent } from './verifications.component';
import { AdminTransactionsComponent } from './transactions.component';
import { AdminEscrowComponent } from './escrow.component';
import { AdminUsersComponent } from './users.component';
import { AdminProductsComponent } from './products.component';
import { AdminDeliveriesComponent } from './deliveries.component';
import { AdminSettingsComponent } from './settings.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '',               component: AdminDashboardComponent },
      { path: 'users',          component: AdminUsersComponent },
      { path: 'verifications',  component: AdminVerificationsComponent },
      { path: 'products',       component: AdminProductsComponent },
      { path: 'transactions',   component: AdminTransactionsComponent },
      { path: 'escrow',         component: AdminEscrowComponent },
      { path: 'deliveries',     component: AdminDeliveriesComponent },
      { path: 'settings',       component: AdminSettingsComponent }
    ]
  }
];
