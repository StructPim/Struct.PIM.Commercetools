using commercetools.Sdk.Api.Models.Common;
using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Models.Attribute;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Struct.PIM.Commercetools.Mapping;

// Commerce attributes not being mapped
// LocalizableTextType
// EnumType
// LocalizableEnumType
// DateType
// TimeType
// TextInputHint
// PlainEnumValue
// LocalizedEnumValue
// AttributeConstraint Enum
public static class AttributeMapping
{
    /// <summary>
    ///     Maps a Struct Json Attribute Structure to Commercetools IAttributeDefinitionDrafts
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="constraint"></param>
    /// <param name="skipAliases"></param>
    /// <param name="includeAliases"></param>
    public static IEnumerable<IAttributeDefinitionDraft> MapDraft(this List<Attribute> attributes, IAttributeConstraintEnum constraint, List<string>? skipAliases = null, List<string>? includeAliases = null)
    {
        var mappedAttributes = new List<IAttributeDefinitionDraft>();
        HandleAttributes(attributes, constraint, null, mappedAttributes, skipAliases, includeAliases);
        return mappedAttributes;
    }


    /// <summary>
    ///     Maps a Struct Json Attribute to Commercetools IAttributeDefinitions
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="constraint"></param>
    /// <param name="skipAliases"></param>
    /// <param name="includeAliases"></param>
    public static IEnumerable<IAttributeDefinition> Map(this List<Attribute> attributes, IAttributeConstraintEnum constraint, List<string>? skipAliases = null, List<string>? includeAliases = null)
    {
        var mappedAttributes = new List<IAttributeDefinition>();
        HandleAttributes(attributes, constraint, mappedAttributes, null, skipAliases, includeAliases);
        return mappedAttributes;
    }


    private static IAttributeDefinitionDraft CreateDraft<TAttribute, TAttributeType>(TAttribute attribute, IAttributeConstraintEnum constraint) where TAttribute : Attribute where TAttributeType : IAttributeType, new()
    {
        var attributeName = attribute.Name.First().Value;
        return new AttributeDefinitionDraft
        {
            Label = new LocalizedString { { "en", attributeName ?? "" } },
            Name = attribute.Alias,
            Type = new TAttributeType
            {
                Name = new TAttributeType().Name
            },
            AttributeConstraint = constraint,
            IsRequired = attribute.Mandatory
        };
    }

    private static IAttributeDefinition Create<TAttribute, TAttributeType>(TAttribute attribute, IAttributeConstraintEnum constraint) where TAttribute : Attribute where TAttributeType : IAttributeType, new()
    {
        var attributeName = attribute.Name.First().Value;
        return new AttributeDefinition
        {
            Label = new LocalizedString { { "en", attributeName ?? "" } },
            Name = attribute.Alias,
            Type = new TAttributeType { Name = new TAttributeType().Name },
            AttributeConstraint = constraint,
            IsRequired = attribute.Mandatory
        };
    }

    private static IAttributeDefinitionDraft CreateCustomObjectDraft<T>(T attribute, IAttributeConstraintEnum constraint) where T : Attribute
    {
        var attributeName = attribute.Name.First().Value;
        return new AttributeDefinitionDraft
        {
            Label = new LocalizedString { { "en", attributeName ?? "" } },
            Name = attribute.Alias,
            Type = new AttributeReferenceType { Name = "reference", ReferenceTypeId = IAttributeReferenceTypeId.KeyValueDocument },
            AttributeConstraint = constraint,
            IsRequired = attribute.Mandatory
        };
    }

    private static IAttributeDefinition CreateCustomObject<T>(T attribute, IAttributeConstraintEnum constraint) where T : Attribute
    {
        var attributeName = attribute.Name.First().Value;
        return new AttributeDefinition
        {
            Label = new LocalizedString { { "en", attributeName ?? "" } },
            Name = attribute.Alias,
            Type = new AttributeReferenceType { Name = "reference", ReferenceTypeId = IAttributeReferenceTypeId.KeyValueDocument },
            AttributeConstraint = constraint,
            IsRequired = attribute.Mandatory
        };
    }

    private static void HandleAttributes(List<Attribute> attributes, IAttributeConstraintEnum constraint, List<IAttributeDefinition>? attributeDefinitions = null, List<IAttributeDefinitionDraft>? attributeDefinitionDrafts = null, List<string>? skipAliases = null, List<string>? includeAliases = null)
    {
        foreach (var attribute in attributes)
        {
            var alias = attribute.Alias.ToLower();

            if (skipAliases != null && skipAliases.Contains(alias))
            {
                continue;
            }

            if (includeAliases != null && (includeAliases.Contains("all") || includeAliases.Contains(alias)))
            {
                switch (attribute.AttributeType)
                {
                    case nameof(TextAttribute):
                        var textAttribute = attribute as TextAttribute;
                        if (textAttribute != null)
                        {
                            if (textAttribute.Localized)
                            {
                                if (attributeDefinitions != null)
                                {
                                    attributeDefinitions.Add(Create<TextAttribute, AttributeLocalizableTextType>(textAttribute, constraint));
                                }
                                else
                                {
                                    attributeDefinitionDrafts?.Add(CreateDraft<TextAttribute, AttributeLocalizableTextType>(textAttribute, constraint));
                                }
                            }
                            else
                            {
                                if (attributeDefinitions != null)
                                {
                                    attributeDefinitions.Add(Create<TextAttribute, AttributeTextType>(textAttribute, constraint));
                                }
                                else
                                {
                                    attributeDefinitionDrafts?.Add(CreateDraft<TextAttribute, AttributeTextType>(textAttribute, constraint));
                                }
                            }

                        }

                        break;
                    case nameof(NumberAttribute):
                        var numberAttribute = attribute as NumberAttribute;
                        if (numberAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(Create<NumberAttribute, AttributeNumberType>(numberAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateDraft<NumberAttribute, AttributeNumberType>(numberAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(BooleanAttribute):
                        var booleanAttribute = attribute as BooleanAttribute;
                        if (booleanAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(Create<BooleanAttribute, AttributeBooleanType>(booleanAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateDraft<BooleanAttribute, AttributeBooleanType>(booleanAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(DateTimeAttribute):
                        var dateTimeAttribute = attribute as DateTimeAttribute;
                        if (dateTimeAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(Create<DateTimeAttribute, AttributeDateTimeType>(dateTimeAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateDraft<DateTimeAttribute, AttributeDateTimeType>(dateTimeAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(MediaAttribute):
                        var mediaAttribute = attribute as MediaAttribute;

                        if (mediaAttribute != null)
                        {

                            if (attributeDefinitions != null)
                            {
                                if (mediaAttribute.AllowMultiselect)
                                {
                                    attributeDefinitions.Add(mediaAttribute.AllowMultiselect ? CreateCustomObject(mediaAttribute, constraint) : Create<MediaAttribute, AttributeTextType>(mediaAttribute, constraint));
                                }

                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(mediaAttribute.AllowMultiselect ? CreateCustomObjectDraft(mediaAttribute, constraint) : CreateDraft<MediaAttribute, AttributeTextType>(mediaAttribute, constraint));

                            }
                        }

                        break;
                    case nameof(ComplexAttribute):
                        var complexAttribute = attribute as ComplexAttribute;
                        if (complexAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(complexAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(complexAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(ListAttribute):
                        var listAttribute = attribute as ListAttribute;
                        if (listAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(listAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(listAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(FixedListAttribute):
                        var fixedListAttribute = attribute as FixedListAttribute;
                        if (fixedListAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(fixedListAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(fixedListAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(VariantReferenceAttribute):
                        var variantReferenceAttribute = attribute as VariantReferenceAttribute;
                        if (variantReferenceAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(variantReferenceAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(variantReferenceAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(ProductReferenceAttribute):
                        var productReferenceAttribute = attribute as ProductReferenceAttribute;
                        if (productReferenceAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(productReferenceAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(productReferenceAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(CategoryReferenceAttribute):
                        var categoryReferenceAttribute = attribute as CategoryReferenceAttribute;
                        if (categoryReferenceAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(categoryReferenceAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(categoryReferenceAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(CollectionReferenceAttribute):
                        var collectionReferenceAttribute = attribute as CollectionReferenceAttribute;
                        if (collectionReferenceAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(collectionReferenceAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(collectionReferenceAttribute, constraint));
                            }
                        }

                        break;
                    case nameof(AttributeReferenceAttribute):
                        var attributeReferenceAttribute = attribute as AttributeReferenceAttribute;
                        if (attributeReferenceAttribute != null)
                        {
                            if (attributeDefinitions != null)
                            {
                                attributeDefinitions.Add(CreateCustomObject(attributeReferenceAttribute, constraint));
                            }
                            else
                            {
                                attributeDefinitionDrafts?.Add(CreateCustomObjectDraft(attributeReferenceAttribute, constraint));
                            }
                        }

                        break;
                }
            }
        }
    }
}