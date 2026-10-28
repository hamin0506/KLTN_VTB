// Isolated fixtures for grouping; no test data is served by the website.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
class Element{constructor(){this.children=[];this.listeners={};this.style={};this.dataset={};this.classList={toggle(){}};}append(x){this.children.push(x);}replaceChildren(){this.children=[];}addEventListener(k,fn){this.listeners[k]=fn;}setAttribute(){}focus(){}}
const buttons=['all','division','detail'].map(mode=>{const e=new Element();e.dataset.employeeMode=mode;return e;});
const context={window:{},document:{createElement:()=>new Element(),querySelectorAll:()=>buttons},Intl};vm.createContext(context);vm.runInContext(fs.readFileSync('wwwroot/js/employee-hierarchy.js','utf8'),context);context.EmployeeHierarchy=context.window.EmployeeHierarchy;
const rows=[{divisionId:1,departmentId:10,parent:'Khối A',category:'Common',code:'DAY_DU',value:2},{divisionId:1,departmentId:11,parent:'Khối A',category:'Common',code:'DAY_DU',value:3},{divisionId:1,departmentId:10,parent:'Khối A',category:'Common',code:'THIEU_HO_SO',value:1},{divisionId:2,departmentId:20,parent:'Khối A',category:'Common',code:'DAY_DU',value:7}];
const original=JSON.stringify(rows),chart={listeners:{},on(k,fn){this.listeners[k]=fn;},setOption(x){this.option=x;},resize(){},clear(){this.option=null;}};
const back=new Element(),crumb=new Element(),picker=new Element(),element=new Element();const h=new context.EmployeeHierarchy(chart,element,back,crumb,picker);h.setRows(rows);
assert.equal(h.mode,'division');assert.equal(chart.option.xAxis.data.join(','),'1,2');assert.equal(picker.hidden,true);buttons[0].listeners.click();assert.equal(h.mode,'all');assert.equal(chart.option.xAxis.data.join(','),'10,11,20');assert.equal(chart.option.xAxis.axisLabel.formatter('10'),'Common');assert.equal(chart.option.series[0].data.join(','),'2,3,7');assert.equal(chart.option.dataZoom,undefined);
buttons[1].listeners.click();assert.equal(chart.option.xAxis.data.join(','),'1,2');assert.equal(chart.option.series[0].data.join(','),'5,7');
chart.listeners.click({componentType:'series',name:'1'});assert.equal(h.mode,'detail');assert.equal(chart.option.xAxis.data.join(','),'10,11');assert.equal(chart.option.tooltip.formatter([{dataIndex:0}]),'Common — Khối A\nĐầy đủ: 2\nThiếu hồ sơ: 1');
back.listeners.click();assert.equal(h.mode,'division');chart.listeners.click({componentType:'xAxis',value:'2'});assert.equal(h.division,2);assert.equal(chart.option.series[0].data[0],7);
buttons[0].listeners.click();assert.equal(chart.option.xAxis.data.length,3);h.setRows(rows.filter(x=>x.divisionId===1));assert.equal(h.division,null);picker.value='1';picker.listeners.change();assert.equal(h.mode,'detail');h.setRows(rows.filter(x=>x.departmentId===11));assert.equal(chart.option.xAxis.data.join(','),'11');
buttons[0].listeners.click();assert.equal(chart.option.series[0].data[0],3);assert.equal(JSON.stringify(rows),original);h.clear();assert.equal(picker.disabled,true);
console.log('PASS three modes, ID identities despite duplicate names, division totals, drill/back, filter scope, tooltip, BLANK and no dataZoom. No real SQL/browser layout tested.');

const wrap=context.EmployeeHierarchy.wrapWords;
for(const label of ['Phòng Phân tích dữ liệu','Trung tâm nền tảng dữ liệu','GIAYKHAMSUCKHOE']){
 const wrapped=wrap(label);assert.equal(wrapped.replace(/\n/g,' '),label);
 for(const word of label.split(' '))assert.ok(wrapped.split(/\s+/).includes(word));
}
assert.equal(wrap('GIAYKHAMSUCKHOE'),'GIAYKHAMSUCKHOE');
h.setRows(rows);assert.equal(chart.option.series[0].barMaxWidth,10);assert.equal(chart.option.xAxis.axisLabel.lineHeight,18);
console.log('PASS whole-word Vietnamese labels, unbroken codes, compact bar width, label spacing.');
