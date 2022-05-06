import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BaseStationService, BaseStationDto, CreateUpdateBaseStationDto } from '@proxy/base-stations';

@Component({
  selector: 'app-basestation',
  templateUrl: './basestation.component.html',
  styleUrls: ['./basestation.component.scss'],
  providers: [ListService],
})
export class BaseStationComponent implements OnInit {
  data = { items: [], totalCount: 0 } as PagedResultDto<BaseStationDto>;
  form: FormGroup;
  selected = {} as BaseStationDto;
  isModalOpen = false;
  constructor(public readonly list: ListService,
    private service: BaseStationService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService) { }

  ngOnInit() {
    
    const streamCreator = (query) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe((response) => {
      console.log(this.list.filter)
      this.data = response;
    });
  }
  create() {
    this.selected = {} as BaseStationDto;
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
      systemName: [this.selected.systemName, Validators.required],
      displayName: [this.selected.displayName, Validators.required],
      bsip: [this.selected.bsip],
      status: [this.selected.status, Validators.required],
      hotel: [this.selected.hotel, Validators.required],
      lotNo: [this.selected.lotNo, Validators.required]
    });
  }

  save() {
    if (this.form.invalid) {
      return;
    }
    const dto: CreateUpdateBaseStationDto = {
      systemName: this.form.get("systemName").value,
      displayName: this.form.get("displayName").value,
      bsip: this.form.get("bsip").value,
      status: this.form.get("status").value,
      hotel: this.form.get("hotel").value,
      lotNo: this.form.get("lotNo").value,
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
