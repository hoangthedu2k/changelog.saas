import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  NgZone,
  PLATFORM_ID,
  signal,
  ViewChild,
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../core/models/user.model';
import { environment } from '../../../../environments/environment';

declare const google: any;
declare const FB: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);
  private zone = inject(NgZone);
  private platformId = inject(PLATFORM_ID);

  @ViewChild('googleBtn') googleBtn!: ElementRef<HTMLDivElement>;

  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required),
  });

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  constructor() {
    afterNextRender(() => {
      this.initGoogle();
      this.initFacebook();
    });
  }

  private initGoogle() {
    if (!isPlatformBrowser(this.platformId)) return;
    const clientId = environment.oauth.googleClientId;
    if (!clientId || typeof google === 'undefined') return;

    google.accounts.id.initialize({
      client_id: clientId,
      callback: (response: { credential: string }) => {
        this.zone.run(() => this.handleOAuth('google', response.credential));
      },
    });

    if (this.googleBtn?.nativeElement) {
      google.accounts.id.renderButton(this.googleBtn.nativeElement, {
        theme: 'outline',
        size: 'large',
        width: 316,
        text: 'signin_with',
        shape: 'rectangular',
        locale: 'en',
      });
    }
  }

  private initFacebook() {
    if (!isPlatformBrowser(this.platformId)) return;
    const appId = environment.oauth.facebookAppId;
    if (!appId || typeof FB === 'undefined') return;

    FB.init({ appId, cookie: true, xfbml: true, version: 'v19.0' });
  }

  loginWithFacebook() {
    if (typeof FB === 'undefined') return;
    FB.login(
      (response: any) => {
        if (response.authResponse?.accessToken) {
          this.zone.run(() => this.handleOAuth('facebook', response.authResponse.accessToken));
        }
      },
      { scope: 'public_profile,email' }
    );
  }

  private handleOAuth(provider: 'google' | 'facebook', token: string) {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.authService.oauthLogin(provider, token).subscribe({
      next: () => this.router.navigate(['/app']),
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Sign-in failed. Please try again.');
      },
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.errorMessage.set(null);
    this.isLoading.set(true);
    const req = this.form.getRawValue() as LoginRequest;
    this.authService.login(req).subscribe({
      next: () => this.router.navigate(['/app']),
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Invalid email or password.');
      },
    });
  }
}
