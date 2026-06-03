import { Component } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RegisterRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {

  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required),
    displayName: new FormControl('', Validators.required),
  });
  
  constructor(private authService: AuthService) {
  }
  onSubmit() {
    if (this.form.invalid) return;
    const req: RegisterRequest = this.form.getRawValue() as RegisterRequest;

    const { email, password, displayName } = this.form.getRawValue();
    this.authService.register(req).subscribe({
      next: () => console.log('Registration successful'),
      error: (err) => console.error('Registration failed', err),
    });
  }

}


