import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guard';
import { roleGuard } from '../../core/auth/role.guard';
import { AdminComponent } from './admin.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '',        component: AdminComponent },
      { path: 'users',          component: AdminComponent },
      { path: 'verifications',  component: AdminComponent },
      { path: 'products',       component: AdminComponent },
      { path: 'transactions',   component: AdminComponent },
      { path: 'escrow',         component: AdminComponent },
      { path: 'deliveries',     component: AdminComponent },
      { path: 'settings',       component: AdminComponent }
    ]
  }
];
