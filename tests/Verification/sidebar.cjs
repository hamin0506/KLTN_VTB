const fs=require('fs'),vm=require('vm'),assert=require('assert');
function test(width,denied=false){
 const classes=new Set(),handlers={};const node=()=>({handlers:{},style:{setProperty(){}},addEventListener(n,f){this.handlers[n]=f},setAttribute(n,v){this[n]=v},focus(){},getBoundingClientRect(){return {top:150}},querySelector(){return null}});
 const toggle=node(),mobile=node(),close=node(),overlay=node(),group=node();group.open=true;
 const sidebar=node();sidebar.querySelector=()=>close;sidebar.querySelectorAll=()=>[group];sidebar.contains=()=>false;
 const root={classList:{toggle(n,value){value=value===undefined?!classes.has(n):value;value?classes.add(n):classes.delete(n)},remove(n){classes.delete(n)},contains(n){return classes.has(n)}}};
 let saved='false';const media={matches:width>=992,addEventListener(n,f){this.change=f}};
 const window={matchMedia:()=>media,innerHeight:900,addEventListener(n,f){handlers[n]=f},dispatchEvent(e){handlers[e.type]?.()}};
 const document={documentElement:root,getElementById:n=>n==='app-sidebar'?sidebar:n==='sidebar-mobile-toggle'?mobile:toggle,querySelector:()=>overlay,addEventListener(){}};
 const storage={getItem(){if(denied)throw Error();return saved},setItem(k,v){if(denied)throw Error();saved=v}};
 vm.runInNewContext(fs.readFileSync('wwwroot/js/sidebar.js','utf8'),{document,window,localStorage:storage,Event});
 toggle.handlers.click();if(media.matches){assert(classes.has('sidebar-compact'));if(!denied)assert.equal(saved,'true');assert.equal(toggle['aria-expanded'],'false');toggle.handlers.click();assert(!classes.has('sidebar-compact'));}
 else{assert(classes.has('sidebar-mobile-open'));assert.equal(sidebar.inert,false);close.handlers.click();assert(!classes.has('sidebar-mobile-open'));assert.equal(sidebar.inert,true);}
}
for(const width of [1920,1440,1366,1024,768,390]){test(width);console.log('PASS sidebar state logic at width '+width+' (simulated, not visual)');}test(1366,true);console.log('PASS localStorage unavailable fallback');
