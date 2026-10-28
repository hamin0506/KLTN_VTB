window.OverviewLayout=class {
 static chartHeight(natural,available,desktop){return desktop?Math.max(natural,available):natural;}
 constructor(root,chart){
  this.root=root;this.chart=chart;this.left=root.querySelector('.dashboard-overview-left');this.card=root.querySelector('.dashboard-documents');this.viewport=this.card.querySelector('.dashboard-chart-viewport');this.element=chart.getDom();
  this.queue=()=>{cancelAnimationFrame(this.frame);this.frame=requestAnimationFrame(()=>this.sync());};
  this.observer=new ResizeObserver(this.queue);this.observer.observe(this.left);this.observer.observe(this.viewport);
  window.addEventListener('resize',this.queue);this.queue();
 }
 sync(){
  const desktop=window.matchMedia('(min-width:1200px)').matches;
  const stack=Math.ceil(this.left.getBoundingClientRect().height)+'px';
  if(this.card.style.getPropertyValue('--overview-stack-height')!==stack)this.card.style.setProperty('--overview-stack-height',stack);
  const natural=Number(this.element.dataset.naturalHeight)||320;
  const height=OverviewLayout.chartHeight(natural,this.viewport.clientHeight,desktop)+'px';
  if(this.element.style.height!==height)this.element.style.height=height;
  this.chart.resize();
 }
 dispose(){cancelAnimationFrame(this.frame);this.observer.disconnect();window.removeEventListener('resize',this.queue);}
};
