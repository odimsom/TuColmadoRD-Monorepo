import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService, EmployeeDto } from '../../../core/services/employee.service';

const ROLE_LABELS: Record<string, { label: string; color: string }> = {
  owner:    { label: 'Dueño',      color: 'bg-purple-500/15 text-purple-300 border-purple-500/25' },
  admin:    { label: 'Admin',      color: 'bg-blue-500/15 text-blue-300 border-blue-500/25' },
  cashier:  { label: 'Cajero',     color: 'bg-emerald-500/15 text-emerald-300 border-emerald-500/25' },
  seller:   { label: 'Vendedor',   color: 'bg-amber-500/15 text-amber-300 border-amber-500/25' },
  delivery: { label: 'Repartidor', color: 'bg-cyan-500/15 text-cyan-300 border-cyan-500/25' },
};

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employees.html',
})
export class Employees implements OnInit {
  private svc = inject(EmployeeService);
  private fb  = inject(FormBuilder);

  employees  = signal<EmployeeDto[]>([]);
  loading    = signal(true);
  saving     = signal(false);
  errorMsg   = signal<string | null>(null);
  showModal  = signal(false);
  editTarget = signal<EmployeeDto | null>(null);

  readonly roles = [
    { value: 'admin',    label: 'Admin' },
    { value: 'cashier',  label: 'Cajero' },
    { value: 'seller',   label: 'Vendedor' },
    { value: 'delivery', label: 'Repartidor' },
  ];

  form = this.fb.group({
    firstName: [''],
    lastName:  [''],
    email:     ['', [Validators.required, Validators.email]],
    password:  ['', [Validators.required, Validators.minLength(8)]],
    role:      ['cashier', Validators.required],
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.list().subscribe({
      next: list => { this.employees.set(list); this.loading.set(false); },
      error: () => { this.errorMsg.set('Error al cargar empleados.'); this.loading.set(false); }
    });
  }

  openCreate(): void {
    this.editTarget.set(null);
    this.form.reset({ role: 'cashier' });
    this.form.controls.email.enable();
    this.form.controls.password.enable();
    this.form.controls.password.setValidators([Validators.required, Validators.minLength(8)]);
    this.form.controls.password.updateValueAndValidity();
    this.showModal.set(true);
  }

  openEdit(emp: EmployeeDto): void {
    this.editTarget.set(emp);
    this.form.patchValue({ firstName: emp.firstName ?? '', lastName: emp.lastName ?? '', email: emp.email, role: emp.role });
    this.form.controls.email.disable();
    this.form.controls.password.clearValidators();
    this.form.controls.password.setValue('');
    this.form.controls.password.updateValueAndValidity();
    this.showModal.set(true);
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.errorMsg.set(null);
    const v = this.form.getRawValue();
    const target = this.editTarget();

    if (target) {
      this.svc.update(target._id, { firstName: v.firstName ?? undefined, lastName: v.lastName ?? undefined, role: v.role! }).subscribe({
        next: updated => {
          this.employees.update(list => list.map(e => e._id === updated._id ? updated : e));
          this.showModal.set(false); this.saving.set(false);
        },
        error: () => { this.errorMsg.set('Error al actualizar.'); this.saving.set(false); }
      });
    } else {
      this.svc.create({ email: v.email!, password: v.password!, firstName: v.firstName ?? undefined, lastName: v.lastName ?? undefined, role: v.role! }).subscribe({
        next: emp => {
          this.employees.update(list => [emp, ...list]);
          this.showModal.set(false); this.saving.set(false);
        },
        error: (err) => {
          const msg = err.error?.error === 'EMAIL_ALREADY_EXISTS' ? 'Este correo ya está registrado.' : 'Error al crear empleado.';
          this.errorMsg.set(msg); this.saving.set(false);
        }
      });
    }
  }

  toggle(emp: EmployeeDto): void {
    const newState = !emp.isActive;
    this.svc.toggle(emp._id, newState).subscribe({
      next: () => this.employees.update(list => list.map(e => e._id === emp._id ? { ...e, isActive: newState } : e)),
      error: () => this.errorMsg.set('Error al cambiar estado.')
    });
  }

  roleInfo(role: string) { return ROLE_LABELS[role] ?? { label: role, color: 'bg-slate-500/15 text-slate-300 border-slate-500/25' }; }
  fullName(e: EmployeeDto): string { return [e.firstName, e.lastName].filter(Boolean).join(' ') || '—'; }
}
