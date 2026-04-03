import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PosLayout } from './pos-layout';

describe('PosLayout', () => {
  let component: PosLayout;
  let fixture: ComponentFixture<PosLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PosLayout],
    }).compileComponents();

    fixture = TestBed.createComponent(PosLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
