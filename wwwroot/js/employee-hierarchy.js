/* All modes use the same authorized SQL aggregate response; identities are database IDs. */
window.EmployeeHierarchy=class {
 // Wrap only at whitespace: never cut a Vietnamese word or a document code.
 static wrapWords(value,limit=10){
  const words=String(value??'').trim().split(/\s+/u),lines=[];let line='';
  for(const word of words){if(line&&line.length+1+word.length>limit){lines.push(line);line=word;}else line+=(line?' ':'')+word;}
  if(line)lines.push(line);return lines.join('\n');
 }
 static shortName(name){return name.replace(/^Khối\s+/iu,'');}
 static project(rows,mode='all',division=null){
  const groups=new Map(),codes=['DAY_DU','THIEU_HO_SO'];
  for(const row of rows){
   if(mode==='detail'&&row.divisionId!==division)continue;
   const id=mode==='division'?row.divisionId:row.departmentId;if(id==null)continue;
   if(!groups.has(id))groups.set(id,{id,label:mode==='division'?EmployeeHierarchy.shortName(row.parent):row.category,fullLabel:mode==='division'?row.parent:row.category+' — '+row.parent,counts:[null,null]});
   const index=codes.indexOf(row.code);if(index>=0&&row.value!=null){const counts=groups.get(id).counts;counts[index]=(counts[index]??0)+row.value;}
  }
  const entries=[...groups.values()];return {entries,categories:entries.map(x=>String(x.id)),series:['Đầy đủ','Thiếu hồ sơ'].map((name,i)=>({name,type:'bar',itemStyle:{color:i===0?'#059669':'#EA580C'},data:entries.map(x=>x.counts[i])}))};
 }
 constructor(chart,element,back,breadcrumb,picker){
  Object.assign(this,{chart,element,back,breadcrumb,picker,rows:[],division:null,mode:'division'});
  this.buttons=[...document.querySelectorAll('[data-employee-mode]')];
  this.buttons.forEach(button=>button.addEventListener('click',()=>{this.mode=button.dataset.employeeMode;this.render();if(this.mode==='detail'&&this.division===null)this.picker.focus();}));
  chart.on('click',event=>{if(this.mode!=='division')return;const id=event.componentType==='xAxis'?event.value:event.componentType==='series'?event.name:null;if(id!==null)this.open(Number(id));});
  back.addEventListener('click',()=>{this.mode='division';this.render();});
  picker.addEventListener('change',()=>{if(picker.value!=='')this.open(Number(picker.value));});
 }
 open(id){if(this.rows.some(row=>row.divisionId===id)){this.division=id;this.mode='detail';this.render();}}
 setRows(rows){this.rows=rows;if(this.division!==null&&!rows.some(row=>row.divisionId===this.division)){this.division=null;if(this.mode==='detail')this.mode='division';}this.render();}
 clear(){this.rows=[];this.division=null;this.back.hidden=true;this.picker.replaceChildren();this.picker.disabled=true;this.breadcrumb.textContent='Chưa tải được dữ liệu';this.chart.clear();}
 render(){
  const view=EmployeeHierarchy.project(this.rows,this.mode,this.division);const divisions=new Map(this.rows.map(row=>[row.divisionId,row.parent]));
  this.picker.hidden=this.mode!=='detail';
  this.back.hidden=this.mode!=='detail';this.breadcrumb.textContent=this.mode==='all'?'Tổng quan / Tất cả phòng ban':this.mode==='division'?'Tổng quan / Theo khối':'Theo khối > '+(this.division===null?'Chọn khối':EmployeeHierarchy.shortName(divisions.get(this.division)));
  this.buttons.forEach(button=>{const active=button.dataset.employeeMode===this.mode;button.setAttribute('aria-pressed',String(active));button.classList.toggle('btn-primary',active);});
  this.picker.replaceChildren();const placeholder=document.createElement('option');placeholder.value='';placeholder.textContent='Chọn khối để xem phòng ban';placeholder.selected=this.division===null;this.picker.append(placeholder);
  for(const [id,name] of divisions){const option=document.createElement('option');option.value=id;option.textContent=EmployeeHierarchy.shortName(name);option.selected=id===this.division;this.picker.append(option);}this.picker.disabled=!this.rows.length;
  this.element.setAttribute('aria-label',this.breadcrumb.textContent);
  const wrapped=view.entries.map(x=>EmployeeHierarchy.wrapWords(x.label));
  const labelLines=Math.max(1,...wrapped.map(x=>x.split('\n').length));
  // Reserve a gap between category labels; unusually long single words stay intact.
  const labelWidth=Math.max(52,...wrapped.flatMap(x=>x.split('\n').map(line=>line.length*7)));
  this.element.style.minWidth=view.entries.length?(60+view.entries.length*(labelWidth+16))+'px':'0';
  this.element.style.height=Math.max(300,210+labelLines*18)+'px';
  const number=new Intl.NumberFormat('vi-VN'),entries=new Map(view.entries.map(x=>[String(x.id),x]));
  this.chart.setOption(view.entries.length?{
   animationDuration:180,aria:{enabled:true},legend:{data:['Đầy đủ','Thiếu hồ sơ'],bottom:4,itemWidth:10,itemHeight:8,textStyle:{fontSize:11}},
   tooltip:{trigger:'axis',renderMode:'richText',confine:true,formatter:items=>{const index=items[0]?.dataIndex;if(index===undefined)return '';return [view.entries[index].fullLabel,...view.series.map(s=>s.name+': '+(s.data[index]===null?'(Trống)':number.format(s.data[index])))].join('\n');}},
   grid:{left:12,right:16,top:20,bottom:42,containLabel:true},
   xAxis:{type:'category',data:view.categories,triggerEvent:true,axisLabel:{interval:0,fontSize:11,lineHeight:18,margin:12,rotate:0,formatter:id=>EmployeeHierarchy.wrapWords(entries.get(String(id))?.label??'')}},
   yAxis:{type:'value',minInterval:1},series:view.series.map(s=>({...s,barMaxWidth:10,barGap:'15%',barCategoryGap:'45%'}))
  }:{title:{text:this.mode==='detail'&&this.division===null?'Chọn một khối để xem phòng ban':'Không có dữ liệu trong phạm vi đã chọn',left:'center',top:'middle',textStyle:{fontSize:13,fontWeight:'normal',color:'#68788a'}}},true);
  this.chart.resize();
 }
};
