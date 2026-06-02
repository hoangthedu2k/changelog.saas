import { computed, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../http/api.service';
import { LoginRequest, LoginResponse, RegisterRequest, User } from '../models/user.model';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private route = inject(Router);
  private api = inject(ApiService);
  private isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  currentUser = signal<User | null>(null);
  isLoggedIn = computed(() => !!this.currentUser());

  login(req: LoginRequest) {
    return this.api.post<LoginResponse>('/auth/login', req).pipe(
      tap(response => {
        this.currentUser.set(response.user);
        if (this.isBrowser) localStorage.setItem('token', response.token);
      })
    );
  }

  logout() {
    if (this.isBrowser) localStorage.removeItem('token');
    this.route.navigate(['/login']);
  }

  initFromStorage() {
    if (!this.isBrowser) return;
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.currentUser.set(payload);
    }
  }

  register(req: RegisterRequest) {
    return this.api.post<LoginResponse>('/auth/register', { ...req }).pipe(
      tap(response => {
        this.currentUser.set(response.user);
        if (this.isBrowser) localStorage.setItem('token', response.token);
      })
    );
  }
}
