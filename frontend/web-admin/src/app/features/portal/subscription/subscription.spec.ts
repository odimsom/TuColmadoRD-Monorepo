import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Subscription } from './subscription';

describe('Subscription', () => {
  let component: Subscription;
  let fixture: ComponentFixture<Subscription>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Subscription],
    }).compileComponents();

    fixture = TestBed.createComponent(Subscription);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
