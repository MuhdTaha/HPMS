import { HttpInterceptorFn } from '@angular/common/http';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const tenantId = localStorage.getItem('tenantId'); // Set this during login

  const authReq = req.clone({
    setHeaders: {
      Authorization: token ? `Bearer ${token}` : '',
      'X-Tenant-Id': tenantId ? tenantId : ''
    }
  });

  return next(authReq);
};
