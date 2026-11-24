(() => {
'use strict';
const modes=['all','group','detail'];
class DocumentTypes {
 constructor(chart,drill,colors,resize){
  this.chart=chart;this.drill=drill;this.colors=colors;this.resize=resize;this.rows=[];this.groups=[];this.first=true;
  const p=new URLSearchParams(location.search);this.mode=modes.includes(p.get('documentMode'))?p.get('documentMode'):'all';this.group=p.get('documentGroup');
  this.pendingScroll=Math.max(0,Number(p.get('documentScroll'))||0);this.pendingPageScroll=Math.max(0,Number(p.get('documentPageScroll'))||0);
  this.select=document.getElementById('document-group');this.viewport=chart.getDom().parentElement;
  this.buttons=[...document.querySelectorAll('[data-document-mode]')];
  this.buttons.forEach(b=>b.addEventListener('click',()=>this.setMode(b.dataset.documentMode)));
  this.select.addEventListener('change',()=>{this.group=this.select.value;this.mode='detail';this.viewport.scrollTop=0;this.render();});
  document.getElementById('document-group-back').addEventListener('click',()=>this.setMode('group'));
  chart.on('click',e=>{
   if(this.mode==='group'&&(e.componentType==='yAxis'||e.componentType==='series'||e.seriesName)){
    const group=e.componentType==='yAxis'?e.value:e.name;if(!this.groups.includes(group))return;this.group=group;this.setMode('detail');
   }else if(this.mode!=='group'&&(e.componentType==='series'||e.seriesName)){
    const row=this.visibleRows.find(x=>String(x.typeId)===String(e.name)&&x.series===e.seriesName);if(row?.typeId){this.persist();this.drill(row.code,row.typeId);}
   }
  });
 }
 setMode(mode){if(!modes.includes(mode))return;this.mode=mode;this.viewport.scrollTop=0;this.render();}
 context(){const p=new URLSearchParams();p.set('documentMode',this.mode);if(this.group)p.set('documentGroup',this.group);p.set('documentScroll',Math.round(this.viewport.scrollTop||0));p.set('documentPageScroll',Math.round(window.scrollY||0));return p;}
 persist(){const p=new URLSearchParams(location.search);for(const k of ['documentMode','documentGroup','documentScroll','documentPageScroll'])p.delete(k);for(const [k,v] of this.context())p.set(k,v);history.replaceState(null,'',location.pathname+'?'+p);}
 setRows(rows){
  const next=new URLSearchParams(location.search);for(const key of [...next.keys()])if(!['DivisionIds','DepartmentIds','Statuses'].includes(key))next.delete(key);next.sort();const sig=next.toString();
  if(!this.first&&this.signature!==sig){this.mode='all';this.group=null;this.viewport.scrollTop=0;}
  const restore=this.first;this.first=false;this.signature=sig;this.rows=rows;this.groups=[...new Set(rows.map(x=>x.group||'Chưa phân nhóm'))];
  if(!this.groups.includes(this.group))this.group=this.groups[0]||null;
  this.render();if(restore){this.viewport.scrollTop=this.pendingScroll;if(this.pendingPageScroll)requestAnimationFrame(()=>window.scrollTo({top:this.pendingPageScroll}));}
 }
 render(){
  const grouped=this.mode==='group';if(this.mode==='detail'&&!this.groups.includes(this.group))this.group=this.groups[0]||null;
  const rows=this.mode==='detail'?this.rows.filter(x=>(x.group||'Chưa phân nhóm')===this.group):this.rows;this.visibleRows=rows;
  const key=x=>grouped?(x.group||'Chưa phân nhóm'):String(x.typeId);const categories=[...new Set(rows.map(key))];
  const states=[...new Set(['Hợp lệ','Sắp hết hạn','Đã hết hạn','Không hợp lệ',...rows.map(x=>x.series||'(Trống)')])];
  const label=k=>grouped?k:(rows.find(x=>String(x.typeId)===k)?.label||rows.find(x=>String(x.typeId)===k)?.category||k);
  const series=states.map(name=>({name,type:'bar',cursor:'pointer',stack:'documents',barWidth:12,itemStyle:{color:this.colors[name]},data:categories.map(k=>rows.filter(x=>key(x)===k&&(x.series||'(Trống)')===name).reduce((n,x)=>n+x.value,0))}));
  const el=this.chart.getDom();el.style.height=Math.max(320,categories.length*55+100)+'px';el.dataset.naturalHeight=parseFloat(el.style.height);
  this.chart.setOption(categories.length?{aria:{enabled:true},tooltip:{trigger:'axis',renderMode:'richText',confine:true,formatter:items=>{const i=items[0]?.dataIndex;if(i===undefined)return '';return [label(categories[i]),'Số bản ghi tài liệu',...series.map(x=>x.name+': '+x.data[i])].join('\n');}},legend:{type:'scroll',bottom:8},grid:{left:20,right:20,top:20,bottom:70,containLabel:true},xAxis:{type:'value',minInterval:1},yAxis:{type:'category',inverse:true,triggerEvent:grouped,data:categories,axisLabel:{fontSize:12,lineHeight:16,formatter:v=>EmployeeHierarchy.wrapWords(label(v),26)}},series}:{title:{text:'Không có dữ liệu trong phạm vi đã chọn',left:'center',top:'middle',textStyle:{fontSize:13}}},true);
  this.buttons.forEach(b=>{const active=b.dataset.documentMode===this.mode;b.classList.toggle('btn-primary',active);b.setAttribute('aria-pressed',String(active));});
  this.select.replaceChildren();for(const group of this.groups){const option=document.createElement('option');option.value=group;option.textContent=group;this.select.append(option);}this.select.value=this.group||'';this.select.hidden=this.mode!=='detail';this.select.disabled=!this.groups.length;
  document.getElementById('document-group-back').hidden=this.mode!=='detail';
  document.getElementById('document-chart-title').textContent='Tổng tài liệu theo loại và trạng thái'+(this.mode==='detail'&&this.group?' – '+this.group:'');
  this.chart.resize();this.resize();this.persist();
 }
}
window.DocumentTypes=DocumentTypes;
})();
