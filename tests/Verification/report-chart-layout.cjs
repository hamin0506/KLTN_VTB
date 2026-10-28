// Layout fixtures only: exercise production chart options without changing SQL data.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const source=fs.readFileSync('wwwroot/js/reports.js','utf8');
const context={percent:new Intl.NumberFormat('vi-VN',{style:'percent'}),number:new Intl.NumberFormat('vi-VN')};
vm.createContext(context);vm.runInContext(source.slice(source.indexOf('function wrapReportLabel('),source.indexOf('function render(')),context);
const element={style:{}},chart={getDom:()=>element,setOption(x){this.option=x;},resize(){}};
const rows=Array.from({length:35},(_,i)=>({name:'Phòng kiểm tra hồ sơ và quản lý tài liệu '+i,value:i}));
context.bar(chart,rows,'#059669',true);
assert.equal(chart.option.yAxis.data.length,35);assert.equal(chart.option.series[0].data[0],0);
assert.equal(chart.option.yAxis.axisLabel.formatter(rows[0].name).replace(/\n/g,' '),rows[0].name);
assert.equal(chart.option.dataZoom,undefined);assert.equal(chart.option.tooltip.confine,true);
assert.ok(parseInt(element.style.height)>430);
context.bar(chart,[{name:'Ngắn',value:1}],'#EA580C');assert.equal(element.style.height,'240px');
console.log('PASS actual report bar: short chart 240px; all 35 long labels/values retained, wrapped labels, confined tooltip, no dataZoom. Layout fixtures only.');

for(const name of ['Phòng Phân tích dữ liệu','Trung tâm nền tảng dữ liệu','GIAYKHAMSUCKHOE']){
 const label=context.wrapReportLabel(name);assert.equal(label.replace(/\n/g,' '),name);
 assert.deepEqual(label.split(/\s+/),name.split(' '));
}
assert.equal(context.wrapReportLabel('GIAYKHAMSUCKHOE',5),'GIAYKHAMSUCKHOE');
assert.equal(chart.option.yAxis.axisLabel.lineHeight,18);
assert.equal(chart.option.yAxis.axisLabel.margin,12);
console.log('PASS Vietnamese words and long single codes preserved; label line height and axis gap.');
