export abstract class AuthDomainError {
  abstract readonly code: string;
  abstract readonly message: string;

  toJSON(): { code: string; message: string } {
    return { code: this.code, message: this.message };
  }
}

export class InvalidCredentialsError extends AuthDomainError {
  readonly code = 'INVALID_CREDENTIALS' as const;
  readonly message = 'Credenciales inválidas.';
}

export class TenantNotFoundError extends AuthDomainError {
  readonly code = 'TENANT_NOT_FOUND' as const;
  readonly message = 'Empresa no encontrada.';
}

export class EmailNotVerifiedError extends AuthDomainError {
  readonly code = 'EMAIL_NOT_VERIFIED' as const;
  readonly message = 'Debes verificar tu correo antes de iniciar sesión.';
}

export class AccountSuspendedError extends AuthDomainError {
  readonly code = 'ACCOUNT_SUSPENDED' as const;
  readonly message = 'Tu cuenta ha sido suspendida.';
}

export class EmailAlreadyExistsError extends AuthDomainError {
  readonly code = 'EMAIL_ALREADY_EXISTS' as const;
  readonly message = 'Este correo ya está registrado.';
}

export class InvalidRoleError extends AuthDomainError {
  readonly code = 'INVALID_ROLE' as const;
  readonly message = 'Rol no válido para empleado.';
}

export class EmployeeNotFoundError extends AuthDomainError {
  readonly code = 'EMPLOYEE_NOT_FOUND' as const;
  readonly message = 'Empleado no encontrado.';
}

export class UserNotFoundError extends AuthDomainError {
  readonly code = 'USER_NOT_FOUND' as const;
  readonly message = 'No hay verificación pendiente para esta cuenta.';
}

export class VerificationCodeExpiredError extends AuthDomainError {
  readonly code = 'CODE_EXPIRED' as const;
  readonly message = 'El código expiró. Solicita uno nuevo.';
}

export class InvalidVerificationCodeError extends AuthDomainError {
  readonly code = 'INVALID_CODE' as const;
  readonly message = 'Código incorrecto.';
}

export class NoPendingVerificationError extends AuthDomainError {
  readonly code = 'NO_PENDING_VERIFICATION' as const;
  readonly message = 'No hay código de verificación pendiente.';
}

export class InvalidStateTransitionError extends AuthDomainError {
  readonly code = 'INVALID_TRANSITION' as const;
  constructor(private readonly detail: string) { super(); }
  get message(): string { return this.detail; }
}

export class InternalAuthError extends AuthDomainError {
  readonly code = 'INTERNAL_ERROR' as const;
  constructor(private readonly detail: string) { super(); }
  get message(): string { return this.detail; }
}
