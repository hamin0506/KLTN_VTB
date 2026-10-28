// Executes the actual document chart configuration with isolated aggregate inputs.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
const source=fs.readFileSync('wwwroot/js/dashboard.js','utf8');const block=source.slice(source.indexOf(' const documents=seriesRows'),source.indexOf(' const accessible='));
const rows=Array.from({length:30},(_,i)=>({category:'TYPE_'+i,series:'Hợp lệ',value:i+1}));
const element={style:{},dataset:{}},chart={setOption(x){this.option=x;},resize(){}};
const colors={'Hợp lệ':'#059669','Không hợp lệ':'#7C3AED','Đã hết hạn':'#DC2626','Sắp hết hạn':'#F59E0B'};
const context={data:{documentsByType:rows},$:()=>element,charts:[null,null,chart],colors,shared:{},overviewLayout:{queue(){}},emptyOption:()=>({}),seriesRows:null};context.window={};vm.createContext(context);vm.runInContext(fs.readFileSync('wwwroot/js/employee-hierarchy.js','utf8'),context);context.EmployeeHierarchy=context.window.EmployeeHierarchy;
vm.runInContext(source.slice(source.indexOf('function seriesRows('),source.indexOf('function render(')),context);
vm.runInContext(block,context);assert.equal(chart.option.dataZoom,undefined);assert.equal(chart.option.yAxis.data.length,30);assert.equal(chart.option.series.length,4);assert.equal(chart.option.series.every(x=>x.stack==='documents'&&x.data.length===30),true);assert.equal(chart.option.grid.right,20);assert.equal(element.style.height,'1060px');assert.equal(chart.option.yAxis.data[29],'TYPE_29');
console.log('PASS actual document chart: 30 categories retained, no dataZoom, four series, dynamic height and compact right margin. Fixtures only, no live data.');
