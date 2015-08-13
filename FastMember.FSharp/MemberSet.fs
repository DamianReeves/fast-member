namespace FastMember
open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open System.Runtime.CompilerServices

type Member = {
  Member:MemberInfo} with
  member this.Name = this.Member.Name
  member this.Type = 
    match this.Member with
    | :? FieldInfo as field -> field.FieldType
    | :? PropertyInfo as property -> property.PropertyType
    | m -> NotSupportedException(m.MemberType.ToString()) |> raise
  member this.IsDefined (attributeType:Type) =
    match attributeType with
    | null -> ArgumentNullException("attributeType") |> raise
    | at -> Attribute.IsDefined(this.Member, attributeType)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<Extension>]
module Member =
  open System
  open System.Reflection

  [<CompiledName("GetMembers")>]
  let getMembers (containingType:Type) = 
    let properties = containingType.GetProperties() |> Seq.cast<MemberInfo>
    let fields = containingType.GetFields() |> Seq.cast<MemberInfo>
    properties |> Seq.append fields |> Seq.map (fun m -> {Member=m} )

  type MemberProvider = {
    GetMembers: Type -> Member seq } with 
    static member Default = { GetMembers=getMembers}
        

type MemberSet(members:Member list) = 
  let memberSequence = lazy(members |> Seq.ofList)
  interface IEnumerable<Member> with
    member x.GetEnumerator(): IEnumerator = memberSequence.Value.GetEnumerator() :> IEnumerator    
    member x.GetEnumerator():IEnumerator<Member> = memberSequence.Value.GetEnumerator()

    