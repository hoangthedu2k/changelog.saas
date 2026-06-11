import { inject } from "@angular/core";
import { AuthService } from "../auth/auth.service";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { HttpInterceptorFn } from "@angular/common/http";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    return next(req).pipe(
        catchError(err => {
            if (err.status === 401) {
                inject(AuthService).logout();
            }
            if (err.status === 403) {
                inject(Router).navigate(['/upgrade']);
            }
            return throwError(() => err);
        })
    );
}
