import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, finalize } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  private activeRequests = 0;

  constructor(private spinner: NgxSpinnerService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (typeof window === 'undefined') {
      return next.handle(req);
    }

    this.activeRequests++;

    if (this.activeRequests === 1) {
      this.spinner.show();
    }

    return next.handle(req).pipe(
      finalize(() => {
        this.activeRequests = Math.max(0, this.activeRequests - 1);

        if (this.activeRequests === 0) {
          this.spinner.hide();
        }
      })
    );
  }
}
