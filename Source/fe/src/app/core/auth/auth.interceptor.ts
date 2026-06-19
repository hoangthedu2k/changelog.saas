import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!isPlatformBrowser(inject(PLATFORM_ID))) return next(req);
  const token = localStorage.getItem('token');
  if (token) {
    return next(req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) }));
  }
  return next(req);
};
