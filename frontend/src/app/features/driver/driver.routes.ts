import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guard';
import { roleGuard } from '../../core/auth/role.guard';
import { DriverDashboardComponent } from './dashboard.component';
import { NotificationsComponent } from '../notifications/notifications.component';
import { DeliveryQueueComponent } from './delivery-queue.component';
import { ActiveDeliveryMapComponent } from './active-delivery-map.component';
import { StatusUpdatesComponent } from './status-updates.component';
import { EarningsComponent } from './earnings.component';

export const DRIVER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Driver', 'Admin'] },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DriverDashboardComponent },
      { path: 'queue', component: DeliveryQueueComponent },
      { path: 'map', component: ActiveDeliveryMapComponent },
      { path: 'status', component: StatusUpdatesComponent },
      { path: 'earnings', component: EarningsComponent },
      { path: 'notifications', component: NotificationsComponent }
    ]
  }
];
