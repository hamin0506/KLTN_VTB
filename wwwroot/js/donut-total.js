(() => {
'use strict';
const number=new Intl.NumberFormat('vi-VN');
// Inputs are the existing mutually exclusive chart buckets, not document table rows.
function total(rows){return rows.reduce((sum,row)=>sum+(Number.isFinite(row.value)?row.value:0),0);}
function graphic(value,width,height,centerY,innerRadius,font='sans-serif'){
 const text=number.format(value),diameter=innerRadius*2;
 const size=Math.max(12,Math.min(28,diameter*.30,diameter*.82/Math.max(1,text.length*.62)));
 return [{id:'donut-total-value',type:'text',silent:true,x:width*.5,y:height*centerY,
 style:{text,fill:'#16365b',fontFamily:font,fontSize:size,fontWeight:700,align:'center',verticalAlign:'middle'}}];
}
window.DonutTotal={total,graphic};
})();
