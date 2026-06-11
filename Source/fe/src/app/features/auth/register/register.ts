import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { RegisterRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required),
    displayName: new FormControl('', Validators.required),
  });

  successMessage = signal('');
  errorMessage = signal('');
  isSubmitting = signal(false);

  onSubmit() {
    if (this.form.invalid) return;
    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const req = this.form.getRawValue() as RegisterRequest;
    this.authService.register(req).subscribe({
      next: () => {
        this.successMessage.set('Account created! Redirecting to login…');
        this.authService.logout();
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.message ?? 'Registration failed. Please try again.');
        this.isSubmitting.set(false);
      },
    });
  }
}


