// Isolated DOM contract test: only filter options; no SQL or business result rows.
const fs=require('fs'),vm=require('vm'),assert=require('assert');
class Element{
 constructor(tag){this.tag=tag;this.children=[];this.dataset={};this.listeners={};this.options=[];this.open=false;this.attributes={};}
 append(...nodes){this.children.push(...nodes);} after(node){this.next=node;} replaceChildren(){this.children=[];} setAttribute(k,v){this.attributes[k]=v;}
 addEventListener(k,fn){this.listeners[k]=fn;} dispatchEvent(e){this.listeners[e.type]?.(e);} focus(){document.activeElement=this;} contains(el){return this===el||this.children.some(x=>x.contains?.(el));}
 get selectedOptions(){return this.options.filter(x=>x.selected);}
}
const label=new Element('label');label.textContent='Trạng thái nhân viên';
const document={activeElement:null,createElement:tag=>new Element(tag),querySelector:()=>label,querySelectorAll:()=>[],addEventListener(){}};
const window={};vm.runInNewContext(fs.readFileSync('wwwroot/js/dashboard-multiselect.js','utf8'),{window,document,Event:class{constructor(type,o){this.type=type;Object.assign(this,o);}}});
const select=new Element('select');select.id='status-filter';select.dataset.placeholder='Tất cả trạng thái';select.options=[{value:'DangLamViec',textContent:'Đang làm việc',selected:true},{value:'TamHoanHDLD',textContent:'Tạm hoãn HĐLĐ',selected:true}];
let changes=0;select.addEventListener('change',()=>{changes++;widget.sync();});const widget=new window.DashboardMultiSelect(select);
assert.equal(select.hidden,true);assert.equal(widget.box.tag,'details');assert.equal(widget.summary.tag,'summary');assert.equal(widget.summary.textContent,'2 đã chọn');
let input=widget.panel.children[0].children[0];input.checked=false;input.listeners.change({stopPropagation(){}});assert.equal(changes,1);assert.equal(select.selectedOptions[0].value,'TamHoanHDLD');assert.equal(widget.summary.textContent,'Tạm hoãn HĐLĐ');
input=widget.panel.children[0].children[0];input.checked=true;input.listeners.change({stopPropagation(){}});assert.equal(select.selectedOptions.length,2);assert.equal(changes,2);
select.options.forEach(x=>x.selected=false);widget.sync();assert.equal(widget.summary.textContent,'Tất cả trạng thái');
select.options=[{value:'2',textContent:'Option',selected:false}];widget.sync();assert.equal(widget.panel.children.length,1);assert.equal(widget.panel.children[0].children[0].dataset.optionValue,'2');
widget.box.open=true;widget.box.listeners.keydown({key:'Escape',preventDefault(){}});assert.equal(widget.box.open,false);assert.equal(document.activeElement,widget.summary);
console.log('PASS dropdown semantic control, multi-selection, original select binding, summary, reset, option replacement and Escape focus. Browser layout and live KPI values not tested.');
