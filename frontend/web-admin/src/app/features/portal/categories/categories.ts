import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { InventoryService, CategoryDto } from '../../../core/services/inventory.service';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.html',
})
export class Categories implements OnInit {
  private inventoryService = inject(InventoryService);
  private fb = inject(FormBuilder);

  categories = signal<CategoryDto[]>([]);
  loading = signal(true);
  saving = signal(false);
  seeding = signal(false);
  errorMsg = signal<string | null>(null);
  successMsg = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.inventoryService.getCategories().subscribe({
      next: (cats) => { this.categories.set(cats); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.errorMsg.set(null);
    this.successMsg.set(null);
    const name = this.form.getRawValue().name.trim();
    this.inventoryService.createCategory(name).subscribe({
      next: () => {
        this.saving.set(false);
        this.form.reset();
        this.successMsg.set(`Categoría "${name}" creada.`);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.detail ?? err?.error?.message ?? 'Error al crear la categoría.');
      },
    });
  }

  seedDefaults(): void {
    if (this.seeding()) return;
    this.seeding.set(true);
    this.errorMsg.set(null);
    this.successMsg.set(null);
    this.inventoryService.seedDefaultCategories().subscribe({
      next: (res) => {
        this.seeding.set(false);
        this.successMsg.set(res.created > 0 ? `${res.created} categorías por defecto agregadas.` : 'Ya existen todas las categorías por defecto.');
        this.load();
      },
      error: (err) => {
        this.seeding.set(false);
        this.errorMsg.set(err?.error?.message ?? 'Error al inicializar categorías.');
      },
    });
  }

  deactivate(cat: CategoryDto): void {
    if (!confirm(`¿Eliminar la categoría "${cat.name}"? Los productos que la usen no se verán afectados.`)) return;
    this.inventoryService.deactivateCategory(cat.id).subscribe({
      next: () => this.load(),
      error: (err) => this.errorMsg.set(err?.error?.message ?? 'Error al eliminar la categoría.'),
    });
  }
}
