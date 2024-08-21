
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemAlertResourceModel } from '@app/resource-models/system-alert.resouce-model';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { HttpClient, HttpParams } from '@angular/common/http';

/**
 * Export system alert API service class.
 * TODO: Write a better class header: system alert API functions.
 */
@Injectable({ providedIn: 'root' })
export class SystemAlertApiService {
  private baseUrl: string;

  public constructor(
    private httpClient: HttpClient,
    appConfigService: AppConfigService,
  ) {
      appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
          this.baseUrl = `${appConfig.portal.api.baseUrl}`;
      });
  }

  public getSystemAlertByTenant(tenant: string): Observable<Array<SystemAlertResourceModel>> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.get<Array<SystemAlertResourceModel>>(`${this.baseUrl}system-alert`, { params });
  }

  public getSystemAlertByProduct(tenant: string, product: string): Observable<any> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.get<SystemAlertResourceModel>(
          `${this.baseUrl}system-alert/product/${product}`,
          { params },
      );
  }

  public getSystemAlertById(tenant: string, systemAlertId: string): Observable<SystemAlertResourceModel> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.get<SystemAlertResourceModel>(`${this.baseUrl}system-alert/${systemAlertId}`, { params });
  }

  public update(tenant: string, systemAlertId: string, model: SystemAlertResourceModel): Observable<Response> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.put<Response>(`${this.baseUrl}system-alert/${systemAlertId}/`, model, { params });
  }

  public disable(tenant: string, systemAlertId: string): Observable<SystemAlertResourceModel> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.put<SystemAlertResourceModel>(
          `${this.baseUrl}system-alert/${systemAlertId}/disable`,
          null,
          { params },
      );
  }

  public enable(tenant: string, systemAlertId: string): Observable<SystemAlertResourceModel> {
      let params: HttpParams = new HttpParams();
      params = params.set("tenant", tenant);
      return this.httpClient.put<SystemAlertResourceModel>(
          `${this.baseUrl}system-alert/${systemAlertId}/enable`,
          null,
          { params },
      );
  }
}
