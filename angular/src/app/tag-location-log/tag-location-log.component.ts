import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TagLocationLogService, TagLocationLogDto } from '@proxy/tag-location-logs';
import { TagDto } from '@proxy/tags';

@Component({
  selector: 'app-tag-location-log',
  templateUrl: './tag-location-log.component.html',
  styleUrls: ['./tag-location-log.component.scss'],
  providers: [ListService],
})
export class TagLocationLogComponent implements OnInit {
  form: FormGroup;
  tags: TagDto[] = [];
  tagLocationLogs: TagLocationLogDto[] = [];
  pollingInterval: any = null;
  constructor(public readonly list: ListService, 
    private service: TagLocationLogService, 
    private fb: FormBuilder,
    private confirmation: ConfirmationService) {}

  ngOnInit() {
    this.buildForm();
    this.getTags();
    this.poll();
  }
  ngOnDestroy() {
    if (this.pollingInterval) clearInterval(this.pollingInterval);
  }
  getTags() {
    this.service.getTags().subscribe((response) => {
      this.tags = response;
    });
  }
  buildForm() {
    this.form = this.fb.group({
      tagId: ['', Validators.required]
    });
  }
  poll(){
    this.pollingInterval = setInterval(() => {
      this.change();
    }, 10000)
  }
  change(){
    if (this.form.valid) {
      const tagId = this.form.get("tagId").value;
      this.service.getTagLocationLogsByTagId(tagId)
        .subscribe((response) => {
          this.tagLocationLogs = response;
        })
    }
  }
  formatTimestamp(log: TagLocationLogDto){
    let startDate = new Date();
    let endDate = new Date(log.creationTime);
    let seconds = (startDate.getTime() - endDate.getTime()) / 1000;
    if(seconds >= 60 && seconds < 3600) return `${Math.floor(seconds/60)} minutes ago.`;
    if(seconds >= 3600 && seconds < 86400) return `${Math.floor(seconds/3600)} hours ago.`;
    if(seconds >= 86400) return `${Math.floor(seconds/86400)} days ago.`;
    return `${Math.floor(seconds)} seconds ago.`;
  }
}
