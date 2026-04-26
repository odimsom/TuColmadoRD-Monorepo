import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customers.html',
})
export class Customers {
  readonly features = [
    { icon: 'icon-[ic--baseline-person-add]', title: 'Registro de Clientes', desc: 'Crea perfiles de clientes con nombre y teléfono para llevar su libreta.' },
    { icon: 'icon-[ic--baseline-credit-card]', title: 'Control de Fiados', desc: 'Registra cuánto le debes cobrar a cada cliente y cuándo fue el último abono.' },
    { icon: 'icon-[ic--baseline-payments]', title: 'Registro de Pagos', desc: 'Marca abonos parciales o pagos completos con fecha y monto exacto.' },
  ];
}
