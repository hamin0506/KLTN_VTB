// Exercise the real module; fixtures never enter VTB2.
const vm=require('vm'),fs=require('fs'),assert=require('assert');
const nodes=new Map();function node(id){if(!nodes.has(id))nodes.set(id,{hidden:false,events:{},children:[],style:{},classList:{toggle(){}},setAttribute(k,v){this[k]=v},addEventListener(k,f){this.events[k]=f},focus(){},textContent:'',replaceChildren(){this.children=[]},append(x){this.children.push(x)}});return nodes.get(id);}
const grid={columns:'510px 510px'};
function chart(controls){const card={querySelector(s){return s==='.card-header'?{offsetHeight:64}:controls?{offsetHeight:100}:null;}};return {dom:{style:{},parentElement:{clientWidth:510},closest(s){return s==='.report-chart-card'?card:grid}},getDom(){return this.dom},resize(){},setOption(o){this.option=o},on(k,f){this[k]=f}};}
const ctx={window:{},document:{getElementById:node,createElement:()=>({})},getComputedStyle:el=>el===grid?{gridTemplateColumns:grid.columns}:{fontFamily:'Inter, sans-serif'},Intl,Map};vm.createContext(ctx);vm.runInContext(fs.readFileSync('wwwroot/js/problem-report.js','utf8'),ctx);
const api=ctx.window.ProblemReport,a=chart(false),b=chart(true);api.init([a,b]);
const data={first:Array.from({length:35},(_,i)=>({name:'Tên tài liệu tiếng Việt đầy đủ '+i,value:i})),organization:[{divisionId:1,divisionName:'Khối một',departmentId:11,departmentName:'Phòng trùng tên',total:1,missing:1},{divisionId:1,divisionName:'Khối một',departmentId:12,departmentName:'Phòng hai',total:99,missing:0},{divisionId:2,divisionName:'Khối hai',departmentId:21,departmentName:'Phòng trùng tên',total:2,missing:1}]};
const labelNames=()=>b.option.yAxis.data.map(k=>b.option.yAxis.axisLabel.formatter(k).replace(/\n/g,' '));
const equalCards=()=>assert.equal(parseInt(a.dom.style.height)+66,parseInt(b.dom.style.height)+166);
api.setData(data);equalCards();assert.equal(a.option.series[0].data.length,35);assert.equal(a.option.series[0].data[0].value,0);assert.equal(a.option.dataZoom,undefined);assert.equal(a.option.series[0].barWidth,12);
assert.equal(b.option.series[0].data[0].value,.99);assert.equal(b.option.yAxis.data.length,2);
for(const key of ['fontFamily','fontSize','fontWeight','color','margin'])assert.equal(a.option.yAxis.axisLabel[key],b.option.yAxis.axisLabel[key]);assert.equal(a.option.xAxis.axisLabel.fontSize,12);assert.equal(b.option.tooltip.textStyle.fontSize,13);
node('problem-mode-all').events.click();assert.equal(b.option.yAxis.data.length,3);assert.equal(new Set(b.option.yAxis.data).size,3);assert.equal(labelNames().filter(n=>n==='Phòng trùng tên').length,2);equalCards();
const index=b.option.yAxis.data.indexOf('21');const tooltip=b.option.tooltip.formatter([{dataIndex:index}]);assert(tooltip.includes('Phòng trùng tên')&&tooltip.includes('Khối hai')&&tooltip.includes('50,00%'));
api.setData(data);assert.equal(b.option.yAxis.data.length,3); // retain all mode across filtering
node('problem-mode-detail').events.click();assert.equal(b.option.title.text,'Chọn đơn vị để xem phòng ban');
node('problem-division').events.change({target:{value:'2'}});assert.deepEqual([...b.option.yAxis.data],['21']);assert(!node('problem-org-back').hidden);equalCards();
node('problem-org-back').events.click();assert(labelNames().includes('Khối một'));
b.click({data:{code:1}});assert.equal(b.option.yAxis.data.length,2);assert(labelNames().includes('Phòng hai'));assert.equal(node('problem-mode-detail')['aria-pressed'],'true');
api.setData({...data,organization:[data.organization[2]]});assert.equal(labelNames()[0],'Khối hai');assert(node('problem-org-back').hidden);
node('expiring-tab').events.click();assert(node('problem-panel').hidden);assert(!node('expiring-panel').hidden);node('problem-tab').events.click();assert(!node('problem-panel').hidden);assert(node('expiring-panel').hidden);
for(const width of [1920,1440,1366,1024,768,390]){a.dom.parentElement.clientWidth=width;b.dom.parentElement.clientWidth=width;grid.columns=width>=1024?'510px 510px':'390px';api.resize();assert.equal(a.option.yAxis.data.length,35);assert.equal(a.dom.style.minWidth,'0');if(width>=1024)equalCards();}
api.clear();a.option=null;a.dom.parentElement.clientWidth=500;api.resize();assert.equal(a.option,null);assert.equal(api.rate(0,0),0);
console.log('PASS equal card heights including controls; shared fonts; three modes, duplicate names by ID, full tooltip, weighted rate, drill/back/filter scope, tabs and resize. Mock DOM only.');
