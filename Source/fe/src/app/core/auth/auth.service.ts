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
        this.currentUser.set({ id: response.userId, email: response.email, displayName: response.displayName } as User);
        if (this.isBrowser) localStorage.setItem('token', response.token);
      })
    );
  }

  logout() {
    if (this.isBrowser) localStorage.removeItem('token');
    this.currentUser.set(null);
    this.route.navigate(['/login']);
  }

  initFromStorage() {
    if (!this.isBrowser) return;
    const token = localStorage.getItem('token');
    if (!token) return;
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
      const payload = JSON.parse(new TextDecoder().decode(bytes));
      if (payload.exp && payload.exp * 1000 < Date.now()) {
        localStorage.removeItem('token');
        return;
      }
      this.currentUser.set({ id: payload.sub, email: payload.email, displayName: payload.name } as User);
    } catch {
      localStorage.removeItem('token');
    }
  }

  register(req: RegisterRequest) {
    return this.api.post<LoginResponse>('/auth/register', { ...req }).pipe(
      tap(response => {
        this.currentUser.set({ id: response.userId, email: response.email, displayName: response.displayName } as User);
        if (this.isBrowser) localStorage.setItem('token', response.token);
      })
    );
  }

  oauthLogin(provider: 'google' | 'facebook', token: string) {
    return this.api.post<LoginResponse>('/auth/oauth', { provider, token }).pipe(
      tap(response => {
        this.currentUser.set({ id: response.userId, email: response.email, displayName: response.displayName } as User);
        if (this.isBrowser) localStorage.setItem('token', response.token);
      })
    );
  }
}
