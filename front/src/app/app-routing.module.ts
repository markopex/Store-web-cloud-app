import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BasketComponent } from './basket/basket/basket.component';
import { LoggedInGuard } from './core/guard/loggedIn.guard';
import { LoginGuard } from './core/guard/login.guard';
import { OrdersComponent } from './orders/orders/orders.component';
import { ProductListComponent } from './products/product-list/product-list.component';
import { LoginComponent } from './user/login/login.component';
import { ProfileComponent } from './user/profile/profile.component';
import { RegisterComponent } from './user/register/register.component';

const routes: Routes = [
  { path: '', pathMatch: "full", redirectTo: 'home' },
  { path: 'home', component: ProductListComponent, canActivate: [LoggedInGuard] },
  { path: 'login', component: LoginComponent, canActivate: [LoginGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [LoginGuard] },
  { path: 'card', component: BasketComponent, canActivate: [LoggedInGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [LoggedInGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [LoggedInGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
