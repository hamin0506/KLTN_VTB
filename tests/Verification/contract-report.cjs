const vm=require('vm'),fs=require('fs'),assert=require('assert');
const nodes=new Map();function node(id){if(!nodes.has(id))nodes.set(id,{hidden:false,events:{},children:[],classList:{toggle(){}},setAttribute(k,v){this[k]=v},addEventListener(k,f){this.events[k]=f},focus(){},replaceChildren(){this.children=[]},append(x){this.children.push(x)}});return nodes.get(id);}
const grid={columns:'500px 500px'};let drillHeight=30;
function chart(bar){const card={querySelector(s){return s==='.card-header'?{offsetHeight:64}:s==='.contract-org-controls'&&bar?{offsetHeight:100}:s==='.contract-drill-actions'&&!bar?{offsetHeight:drillHeight}:null;}};return {dom:{style:{},parentElement:{clientWidth:500},closest(s){return s==='.report-chart-card'?card:grid}},getDom(){return this.dom},resize(){},setOption(o){this.option=o},on(k,f){this[k]=f}};}
const ctx={window:{},document:{getElementById:node,createElement:()=>({}),querySelector:node},getComputedStyle:el=>el===grid?{gridTemplateColumns:grid.columns}:{fontFamily:'Inter, sans-serif'},Intl,Map};vm.createContext(ctx);vm.runInContext(fs.readFileSync('wwwroot/js/donut-total.js','utf8'),ctx);ctx.DonutTotal=ctx.window.DonutTotal;vm.runInContext(fs.readFileSync('wwwroot/js/contract-report.js','utf8'),ctx);
const api=ctx.window.ContractReport,a=chart(true),b=chart(false);let chosenDepartment;api.init([a,b],{HOP_LE:'#059669',CAN_KY_KET_TIEP:'#D97706',THIEU_HDXDTH1:'#DC2626'},id=>chosenDepartment=id);
const organization=[{divisionId:1,divisionName:'Khối một',departmentId:11,departmentName:'Phòng trùng',count:2},{divisionId:1,divisionName:'Khối một',departmentId:12,departmentName:'Phòng hai',count:3},{divisionId:2,divisionName:'Khối hai',departmentId:21,departmentName:'Phòng trùng',count:1}];
const data={second:[{name:'Hợp lệ',code:'HOP_LE',value:9}],contractOrganization:organization};api.setData(data);
const equal=()=>assert.equal(parseInt(a.dom.style.height)+166,parseInt(b.dom.style.height)+66+drillHeight);
equal();assert.equal(a.option.series[0].data[0].value,5);assert.equal(b.option.series[0].data[0].value,9);assert.equal(b.option.series[0].data[0].code,'HOP_LE');assert.equal(b.option.series[0].data[0].itemStyle.color,'#059669');assert.equal(b.option.legend.textStyle.fontFamily,a.option.yAxis.axisLabel.fontFamily);assert.equal(a.option.series[0].barWidth,12);assert.equal(a.option.dataZoom,undefined);
node('contract-mode-all').events.click();assert.equal(a.option.yAxis.data.length,3);assert.equal(new Set(a.option.yAxis.data).size,3);const tooltip=a.option.tooltip.formatter([{dataIndex:a.option.yAxis.data.indexOf('21')}]);assert(tooltip.includes('Phòng trùng')&&tooltip.includes('Khối hai')&&tooltip.includes('1'));equal();
node('contract-mode-detail').events.click();assert(a.option.title.text.includes('Chọn khối'));node('contract-division').events.change({target:{value:'1'}});assert.equal(a.option.yAxis.data.length,2);
node('contract-org-back').events.click();a.click({data:{code:2}});assert.deepEqual([...a.option.yAxis.data],['21']);api.setData({...data,contractOrganization:organization.slice(0,2)});assert(node('contract-org-back').hidden);
node('expiredContracts-tab').events.click();assert(node('contractIssues-panel').hidden);assert(!node('expiredContracts-panel').hidden);node('contractIssues-tab').events.click();assert(node('expiredContracts-panel').hidden);
api.setData({...data,contractOrganization:Array.from({length:35},(_,i)=>({divisionId:1,divisionName:'Khối một',departmentId:i+1,departmentName:'Phòng ban dài '+i,count:i}))});node('contract-mode-all').events.click();assert.equal(a.option.series[0].data.length,35);equal();
drillHeight=130;node('.contract-drill-actions').events.toggle();equal();
for(const width of [1920,1440,1366,768,390]){a.dom.parentElement.clientWidth=width;b.dom.parentElement.clientWidth=width;grid.columns=width>=1024?'500px 500px':'390px';api.resize();assert.equal(a.option.yAxis.data.length,35);if(width>=1024)equal();}
console.log('PASS contract modes, distinct category IDs, counts, full rows, matched heights, donut values/colors/state, fonts, tab state and resize. Mock DOM only.');

assert.equal(b.option.graphic[0].style.text,'9');assert(b.option.graphic.every(x=>x.silent));api.setData({second:[],contractOrganization:[]});assert.equal(b.option.graphic[0].style.text,'0');console.log('PASS dynamic donut total and empty state.');

assert.equal(b.option.graphic.length,1);assert.equal(b.option.graphic[0].y,parseFloat(b.dom.style.height)*.45);

api.setData(data);node('contract-mode-all').events.click();a.click({data:{code:21}});assert.equal(chosenDepartment,21);console.log('PASS contract department click passes exact ID to existing detail navigation.');

const slices=[{name:'Cần ký kết tiếp',code:'CAN_KY_KET_TIEP',value:3},{name:'Thiếu HĐXĐTH1',code:'THIEU_HDXDTH1',value:2}];
for(const second of [slices,[...slices].reverse(),[slices[1]],[slices[0]]]){api.setData({second,contractOrganization:organization});for(const point of b.option.series[0].data)assert.equal(point.itemStyle.color,point.code==='CAN_KY_KET_TIEP'?'#D97706':'#DC2626');}
console.log('PASS contract slice colors remain keyed by code after reorder and filtering.');
