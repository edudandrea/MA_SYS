/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { PagamentosService } from './Pagamentos.service';

describe('Service: Pagamentos', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PagamentosService]
    });
  });

  it('should ...', inject([PagamentosService], (service: PagamentosService) => {
    expect(service).toBeTruthy();
  }));
});
