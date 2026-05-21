import { Routes } from '@angular/router';
import { authGuard } from '../../core/auth/auth.guard';
import { roleGuard } from '../../core/auth/role.guard';
import { SupplierDashboardComponent } from './dashboard.component';
import { MarketplaceComponent } from '../marketplace/marketplace.component';
import { OrdersComponent } from '../orders/orders.component';
import { NotificationsComponent } from '../notifications/notifications.component';
import { InventoryComponent } from './inventory.component';
import { DeliverySchedulingComponent } from './delivery-scheduling.component';
import { RevenueAnalyticsComponent } from './revenue-analytics.component';

export const SUPPLIER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Supplier', 'Admin'] },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: SupplierDashboardComponent },
      { path: 'products', component: MarketplaceComponent },
      { path: 'inventory', component: InventoryComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'deliveries', component: DeliverySchedulingComponent },
      { path: 'revenue', component: RevenueAnalyticsComponent },
      { path: 'notifications', component: NotificationsComponent }
    ]
  }
];
