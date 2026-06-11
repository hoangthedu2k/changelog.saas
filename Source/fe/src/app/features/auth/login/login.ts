import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../core/models/user.model';

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

  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required),
  });

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

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
