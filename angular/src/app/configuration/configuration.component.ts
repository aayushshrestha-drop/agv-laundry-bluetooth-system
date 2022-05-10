import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfigurationService, ConfigurationDto, CreateUpdateConfigurationDto } from '@proxy/configurations';

@Component({
  selector: 'app-configuration',
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.scss'],
  providers: [ListService],
})
export class ConfigurationComponent implements OnInit {
  data = { items: [], totalCount: 0 } as PagedResultDto<ConfigurationDto>;
  form: FormGroup;
  selected = {} as ConfigurationDto;
  isModalOpen = false;

  constructor(public readonly list: ListService, 
    private service: ConfigurationService, 
    private fb: FormBuilder,
    private confirmation: ConfirmationService) {}

  ngOnInit() {
    const streamCreator = (query) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.data = response;
    });
  }
  create() {
    this.selected = {} as ConfigurationDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  edit(id: string) {
    this.service.get(id).subscribe((asset) => {
      this.selected = asset;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  buildForm() {
    this.form = this.fb.group({
      key: [this.selected.key, Validators.required],
      value: [this.selected.value, Validators.required]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }
    const dto: CreateUpdateConfigurationDto = {
      key: this.form.value.key,
      value: this.form.value.value
    };
    const request = this.selected.id
      ? this.service.update(this.selected.id, dto)
      : this.service.create(dto);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
    });
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => this.list.get());
      }
    });
  }
}
