using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
namespace Vtb.Hr.Web.Models;
// Normalize only explicitly provided input before model validation. Missing input
// does not overwrite the tracked value when editing another field.
public sealed class StorageFormModelBinder:IModelBinder
{
 public Task BindModelAsync(ModelBindingContext context){
  var input=context.ValueProvider.GetValue(context.ModelName);
  if(input==ValueProviderResult.None)return Task.CompletedTask;
  var normalized=DocumentStorageForms.Normalize(input.FirstValue);
  context.ModelState.SetModelValue(context.ModelName,new ValueProviderResult(new StringValues(normalized??""),input.Culture));
  context.Result=ModelBindingResult.Success(normalized);
  return Task.CompletedTask;
 }
}
