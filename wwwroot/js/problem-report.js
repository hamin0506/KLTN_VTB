(() => {
'use strict';
const number=new Intl.NumberFormat('vi-VN'),percent=new Intl.NumberFormat('vi-VN',{style:'percent',minimumFractionDigits:2,maximumFractionDigits:2});
let view='division',selected=null,organization=[],first=[],charts=[],lastWidth=[],loading=true;
function rows(){
 if(view!=='division')return organization.filter(x=>view==='all'||x.divisionId===selected).map(x=>({name:x.departmentName,division:x.divisionName,value:rate(x.total,x.missing),code:x.departmentId})).sort(compare);
 const divisions=new Map();for(const x of organization){const d=divisions.get(x.divisionId)||{name:x.divisionName,code:x.divisionId,total:0,missing:0};d.total+=x.total;d.missing+=x.missing;divisions.set(x.divisionId,d);}
 return [...divisions.values()].map(x=>({...x,value:rate(x.total,x.missing)})).sort(compare);
}
function rate(total,missing){return total===0?0:(total-missing)/total;}
function compare(a,b){return b.value-a.value||a.name.localeCompare(b.name,'vi')||a.code-b.code;}
function layout(chart,data){
 const el=chart.getDom(),width=el.parentElement.clientWidth||300;
 const labelWidth=Math.min(270,Math.max(120,width*.48)),limit=Math.max(14,Math.floor(labelWidth/6.5));
 const wrap=value=>{const lines=[];let line='';for(const word of String(value).split(/\s+/u)){if(line&&line.length+word.length+1>limit){lines.push(line);line=word;}else line+=(line?' ':'')+word;}if(line)lines.push(line);return lines.join('\n');};
 const lineCount=Math.max(1,...data.map(x=>wrap(x.name).split('\n').length));
 const card=el.closest('.report-chart-card');
 const overhead=(card.querySelector('.card-header')?.offsetHeight||0)+(card.querySelector('.problem-org-controls')?.offsetHeight||0)+2;
 return {wrap,overhead,height:Math.max(200,data.length*Math.max(24,lineCount*16+8)+54)};
}
function draw(chart,data,color,isRate,metrics,height,font){
 const el=chart.getDom();el.style.minWidth='0';el.style.height=height+'px';chart.resize();
 const axis={fontFamily:font,fontSize:12,fontWeight:400,color:'#52667b',lineHeight:16,margin:10};
 const keys=data.map((x,i)=>String(x.code??'document-'+i)),names=new Map(keys.map((key,i)=>[key,data[i].name]));
 chart.setOption(data.length?{textStyle:{fontFamily:font,fontSize:12,fontWeight:400,color:'#52667b'},aria:{enabled:true},tooltip:{trigger:'axis',renderMode:'richText',confine:true,textStyle:{fontFamily:font,fontSize:13,fontWeight:400,color:'#182433'},formatter:params=>{const point=data[params[0]?.dataIndex];if(!point)return '';return point.name+(point.division?'\nĐơn vị: '+point.division:'')+'\n'+(isRate?'Tỷ lệ đáp ứng: ':'Số lượng: ')+(point.value==null?'(Trống)':isRate?percent.format(point.value):number.format(point.value));}},grid:{left:12,right:20,top:12,bottom:24,containLabel:true},xAxis:{type:'value',min:0,axisLabel:{...axis,...(isRate?{formatter:v=>percent.format(v)}:{})},...(isRate?{max:1}:{minInterval:1})},yAxis:{type:'category',inverse:true,data:keys,axisLabel:{...axis,formatter:key=>metrics.wrap(names.get(String(key))||key)}},series:[{name:isRate?'Tỷ lệ đáp ứng':'Số lượng',type:'bar',barWidth:12,barCategoryGap:'50%',data:data.map(x=>({value:x.value,code:x.code})),itemStyle:{color}}]}:{title:{text:isRate&&view==='detail'&&selected===null?'Chọn đơn vị để xem phòng ban':'Không có dữ liệu',left:'center',top:'middle',textStyle:{fontFamily:font,fontSize:13,fontWeight:400}}},true);
}
function render(){
 if(charts.length!==2||loading)return;
 const chosen=organization.find(x=>x.divisionId===selected),right=rows();
 document.getElementById('problem-org-title').textContent=view==='division'?'Tỷ lệ đáp ứng hồ sơ theo đơn vị':'Tỷ lệ đáp ứng hồ sơ theo phòng ban';
 document.getElementById('problem-org-path').textContent=view==='division'?'Theo đơn vị · Nhấn thanh để xem phòng ban':view==='all'?'Tất cả phòng ban trong phạm vi lọc':chosen?'Chi tiết đơn vị / '+chosen.divisionName:'Chọn đơn vị để xem phòng ban';
 document.getElementById('problem-org-back').hidden=view==='division';
 document.getElementById('problem-division-label').hidden=view!=='detail';
 for(const name of ['division','all','detail']){const button=document.getElementById('problem-mode-'+name);button.classList.toggle('active',view===name);button.setAttribute('aria-pressed',String(view===name));}
 const select=document.getElementById('problem-division');select.replaceChildren();
 const placeholder=document.createElement('option');placeholder.value='';placeholder.textContent='Chọn đơn vị';select.append(placeholder);
 const divisions=new Map(organization.map(x=>[x.divisionId,x.divisionName]));for(const [id,name] of divisions){const option=document.createElement('option');option.value=String(id);option.textContent=name;select.append(option);}select.value=selected===null?'':String(selected);
 const metrics=[layout(charts[0],first),layout(charts[1],right)];
 const grid=charts[0].getDom().closest('.report-charts-grid');
 const desktop=getComputedStyle(grid).gridTemplateColumns.trim().split(/\s+/).length>1;
 const common=Math.max(...metrics.map(m=>m.height+m.overhead));
 const font=getComputedStyle(document.getElementById('secondary-report')).fontFamily;
 draw(charts[0],first,'#EA580C',false,metrics[0],desktop?common-metrics[0].overhead:metrics[0].height,font);
 draw(charts[1],right,'#059669',true,metrics[1],desktop?common-metrics[1].overhead:metrics[1].height,font);
 charts.forEach((c,i)=>lastWidth[i]=c.getDom().parentElement.clientWidth);
}
function init(value){
 charts=value;
 charts[1].on('click',event=>{if(view!=='division')return;const id=event.data?.code;if(organization.some(x=>x.divisionId===id)){selected=id;view='detail';render();}});
 document.getElementById('problem-org-back').addEventListener('click',()=>{view='division';selected=null;render();});
 for(const name of ['division','all','detail'])document.getElementById('problem-mode-'+name).addEventListener('click',()=>{view=name;render();});
 document.getElementById('problem-division').addEventListener('change',e=>{selected=e.target.value===''?null:Number(e.target.value);render();});
 const tabs=['problem','expiring'];function activate(key){for(const name of tabs){const active=name===key,tab=document.getElementById(name+'-tab');tab.classList.toggle('active',active);tab.setAttribute('aria-selected',String(active));tab.tabIndex=active?0:-1;document.getElementById(name+'-panel').hidden=!active;}}
 for(const name of tabs){const tab=document.getElementById(name+'-tab');tab.addEventListener('click',()=>activate(name));tab.addEventListener('keydown',e=>{if(['ArrowLeft','ArrowRight','Home','End'].includes(e.key)){e.preventDefault();const next=e.key==='Home'?'problem':e.key==='End'?'expiring':name==='problem'?'expiring':'problem';activate(next);document.getElementById(next+'-tab').focus();}});}
}
window.ProblemReport={init,clear(){loading=true;},setData(data){loading=false;first=data.first;organization=data.organization||[];if(selected!==null&&!organization.some(x=>x.divisionId===selected)){selected=null;if(view==='detail')view='division';}render();},reset(){selected=null;view='division';},resize(){if(charts.some((c,i)=>c.getDom().parentElement.clientWidth!==lastWidth[i]))render();},rate};
})();
