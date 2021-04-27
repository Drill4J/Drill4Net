#ifndef KONAN_AGENT_CONNECTOR_H
#define KONAN_AGENT_CONNECTOR_H
#ifdef __cplusplus
extern "C" {
#endif
#ifdef __cplusplus
typedef bool            agent_connector_KBoolean;
#else
typedef _Bool           agent_connector_KBoolean;
#endif
typedef unsigned short     agent_connector_KChar;
typedef signed char        agent_connector_KByte;
typedef short              agent_connector_KShort;
typedef int                agent_connector_KInt;
typedef long long          agent_connector_KLong;
typedef unsigned char      agent_connector_KUByte;
typedef unsigned short     agent_connector_KUShort;
typedef unsigned int       agent_connector_KUInt;
typedef unsigned long long agent_connector_KULong;
typedef float              agent_connector_KFloat;
typedef double             agent_connector_KDouble;
typedef float __attribute__ ((__vector_size__ (16))) agent_connector_KVector128;
typedef void*              agent_connector_KNativePtr;
struct agent_connector_KType;
typedef struct agent_connector_KType agent_connector_KType;

typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Byte;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Short;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Int;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Long;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Float;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Double;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Char;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Boolean;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Unit;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstEntity;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_collections_List;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Any;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstEntity_$serializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlinx_serialization_SerialDescriptor;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Array;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlinx_serialization_Decoder;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlinx_serialization_Encoder;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstEntity_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlinx_serialization_KSerializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstMethod;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstMethod_$serializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_AstMethod_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_CoverMessage;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_CoverMessage_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitDataPart;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitDataPart_$serializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitDataPart_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitInfo;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitInfo_$serializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_InitInfo_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_Initialized;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_Initialized_$serializer;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_Initialized_Companion;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_com_epam_drill_common_AgentType;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_kotlin_Function2;
typedef struct {
  agent_connector_KNativePtr pinned;
} agent_connector_kref_com_epam_drill_core_ws_WsSocket;

extern void initialize_agent(const char* agentId, const char* adminAddress, const char* buildVersion, const char* groupId, const char* instanceId, void* function);
extern void sendMessage(const char* messageType, const char* destination, const char* content);
extern void sendPluginMessage(const char* pluginId, const char* content);

typedef struct {
  /* Service functions. */
  void (*DisposeStablePointer)(agent_connector_KNativePtr ptr);
  void (*DisposeString)(const char* string);
  agent_connector_KBoolean (*IsInstance)(agent_connector_KNativePtr ref, const agent_connector_KType* type);
  agent_connector_kref_kotlin_Byte (*createNullableByte)(agent_connector_KByte);
  agent_connector_kref_kotlin_Short (*createNullableShort)(agent_connector_KShort);
  agent_connector_kref_kotlin_Int (*createNullableInt)(agent_connector_KInt);
  agent_connector_kref_kotlin_Long (*createNullableLong)(agent_connector_KLong);
  agent_connector_kref_kotlin_Float (*createNullableFloat)(agent_connector_KFloat);
  agent_connector_kref_kotlin_Double (*createNullableDouble)(agent_connector_KDouble);
  agent_connector_kref_kotlin_Char (*createNullableChar)(agent_connector_KChar);
  agent_connector_kref_kotlin_Boolean (*createNullableBoolean)(agent_connector_KBoolean);
  agent_connector_kref_kotlin_Unit (*createNullableUnit)(void);

  /* User functions. */
  struct {
    struct {
      void (*main)();
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_AstEntity (*AstEntity)(agent_connector_KInt seen1, const char* path, const char* name, agent_connector_kref_kotlin_collections_List methods, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        agent_connector_kref_AstEntity (*AstEntity_)(const char* path, const char* name, agent_connector_kref_kotlin_collections_List methods);
        agent_connector_kref_kotlin_collections_List (*get_methods)(agent_connector_kref_AstEntity thiz);
        const char* (*get_name)(agent_connector_kref_AstEntity thiz);
        const char* (*get_path)(agent_connector_kref_AstEntity thiz);
        const char* (*component1)(agent_connector_kref_AstEntity thiz);
        const char* (*component2)(agent_connector_kref_AstEntity thiz);
        agent_connector_kref_kotlin_collections_List (*component3)(agent_connector_kref_AstEntity thiz);
        agent_connector_kref_AstEntity (*copy)(agent_connector_kref_AstEntity thiz, const char* path, const char* name, agent_connector_kref_kotlin_collections_List methods);
        agent_connector_KBoolean (*equals)(agent_connector_kref_AstEntity thiz, agent_connector_kref_kotlin_Any other);
        agent_connector_KInt (*hashCode)(agent_connector_kref_AstEntity thiz);
        const char* (*toString)(agent_connector_kref_AstEntity thiz);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_AstEntity_$serializer (*_instance)();
          agent_connector_kref_kotlinx_serialization_SerialDescriptor (*get_descriptor)(agent_connector_kref_AstEntity_$serializer thiz);
          agent_connector_kref_kotlin_Array (*childSerializers)(agent_connector_kref_AstEntity_$serializer thiz);
          agent_connector_kref_AstEntity (*deserialize)(agent_connector_kref_AstEntity_$serializer thiz, agent_connector_kref_kotlinx_serialization_Decoder decoder);
          void (*serialize)(agent_connector_kref_AstEntity_$serializer thiz, agent_connector_kref_kotlinx_serialization_Encoder encoder, agent_connector_kref_AstEntity value);
        } $serializer;
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_AstEntity_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_AstEntity_Companion thiz);
        } Companion;
      } AstEntity;
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_AstMethod (*AstMethod)(agent_connector_KInt seen1, const char* name, agent_connector_kref_kotlin_collections_List params, const char* returnType, agent_connector_KInt count, agent_connector_kref_kotlin_collections_List probes, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        agent_connector_kref_AstMethod (*AstMethod_)(const char* name, agent_connector_kref_kotlin_collections_List params, const char* returnType, agent_connector_KInt count, agent_connector_kref_kotlin_collections_List probes);
        agent_connector_KInt (*get_count)(agent_connector_kref_AstMethod thiz);
        const char* (*get_name)(agent_connector_kref_AstMethod thiz);
        agent_connector_kref_kotlin_collections_List (*get_params)(agent_connector_kref_AstMethod thiz);
        agent_connector_kref_kotlin_collections_List (*get_probes)(agent_connector_kref_AstMethod thiz);
        const char* (*get_returnType)(agent_connector_kref_AstMethod thiz);
        const char* (*component1)(agent_connector_kref_AstMethod thiz);
        agent_connector_kref_kotlin_collections_List (*component2)(agent_connector_kref_AstMethod thiz);
        const char* (*component3)(agent_connector_kref_AstMethod thiz);
        agent_connector_KInt (*component4)(agent_connector_kref_AstMethod thiz);
        agent_connector_kref_kotlin_collections_List (*component5)(agent_connector_kref_AstMethod thiz);
        agent_connector_kref_AstMethod (*copy)(agent_connector_kref_AstMethod thiz, const char* name, agent_connector_kref_kotlin_collections_List params, const char* returnType, agent_connector_KInt count, agent_connector_kref_kotlin_collections_List probes);
        agent_connector_KBoolean (*equals)(agent_connector_kref_AstMethod thiz, agent_connector_kref_kotlin_Any other);
        agent_connector_KInt (*hashCode)(agent_connector_kref_AstMethod thiz);
        const char* (*toString)(agent_connector_kref_AstMethod thiz);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_AstMethod_$serializer (*_instance)();
          agent_connector_kref_kotlinx_serialization_SerialDescriptor (*get_descriptor)(agent_connector_kref_AstMethod_$serializer thiz);
          agent_connector_kref_kotlin_Array (*childSerializers)(agent_connector_kref_AstMethod_$serializer thiz);
          agent_connector_kref_AstMethod (*deserialize)(agent_connector_kref_AstMethod_$serializer thiz, agent_connector_kref_kotlinx_serialization_Decoder decoder);
          void (*serialize)(agent_connector_kref_AstMethod_$serializer thiz, agent_connector_kref_kotlinx_serialization_Encoder encoder, agent_connector_kref_AstMethod value);
        } $serializer;
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_AstMethod_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_AstMethod_Companion thiz);
        } Companion;
      } AstMethod;
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_CoverMessage (*CoverMessage)(agent_connector_KInt seen1, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_CoverMessage_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_CoverMessage_Companion thiz);
        } Companion;
      } CoverMessage;
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_InitDataPart (*InitDataPart)(agent_connector_KInt seen1, agent_connector_kref_kotlin_collections_List astEntities, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        agent_connector_kref_InitDataPart (*InitDataPart_)(agent_connector_kref_kotlin_collections_List astEntities);
        agent_connector_kref_kotlin_collections_List (*get_astEntities)(agent_connector_kref_InitDataPart thiz);
        agent_connector_kref_kotlin_collections_List (*component1)(agent_connector_kref_InitDataPart thiz);
        agent_connector_kref_InitDataPart (*copy)(agent_connector_kref_InitDataPart thiz, agent_connector_kref_kotlin_collections_List astEntities);
        agent_connector_KBoolean (*equals)(agent_connector_kref_InitDataPart thiz, agent_connector_kref_kotlin_Any other);
        agent_connector_KInt (*hashCode)(agent_connector_kref_InitDataPart thiz);
        const char* (*toString)(agent_connector_kref_InitDataPart thiz);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_InitDataPart_$serializer (*_instance)();
          agent_connector_kref_kotlinx_serialization_SerialDescriptor (*get_descriptor)(agent_connector_kref_InitDataPart_$serializer thiz);
          agent_connector_kref_kotlin_Array (*childSerializers)(agent_connector_kref_InitDataPart_$serializer thiz);
          agent_connector_kref_InitDataPart (*deserialize)(agent_connector_kref_InitDataPart_$serializer thiz, agent_connector_kref_kotlinx_serialization_Decoder decoder);
          void (*serialize)(agent_connector_kref_InitDataPart_$serializer thiz, agent_connector_kref_kotlinx_serialization_Encoder encoder, agent_connector_kref_InitDataPart value);
        } $serializer;
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_InitDataPart_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_InitDataPart_Companion thiz);
        } Companion;
      } InitDataPart;
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_InitInfo (*InitInfo)(agent_connector_KInt seen1, agent_connector_KInt classesCount, const char* message, agent_connector_KBoolean init, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        agent_connector_kref_InitInfo (*InitInfo_)(agent_connector_KInt classesCount, const char* message, agent_connector_KBoolean init);
        agent_connector_KInt (*get_classesCount)(agent_connector_kref_InitInfo thiz);
        agent_connector_KBoolean (*get_init)(agent_connector_kref_InitInfo thiz);
        const char* (*get_message)(agent_connector_kref_InitInfo thiz);
        agent_connector_KInt (*component1)(agent_connector_kref_InitInfo thiz);
        const char* (*component2)(agent_connector_kref_InitInfo thiz);
        agent_connector_KBoolean (*component3)(agent_connector_kref_InitInfo thiz);
        agent_connector_kref_InitInfo (*copy)(agent_connector_kref_InitInfo thiz, agent_connector_KInt classesCount, const char* message, agent_connector_KBoolean init);
        agent_connector_KBoolean (*equals)(agent_connector_kref_InitInfo thiz, agent_connector_kref_kotlin_Any other);
        agent_connector_KInt (*hashCode)(agent_connector_kref_InitInfo thiz);
        const char* (*toString)(agent_connector_kref_InitInfo thiz);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_InitInfo_$serializer (*_instance)();
          agent_connector_kref_kotlinx_serialization_SerialDescriptor (*get_descriptor)(agent_connector_kref_InitInfo_$serializer thiz);
          agent_connector_kref_kotlin_Array (*childSerializers)(agent_connector_kref_InitInfo_$serializer thiz);
          agent_connector_kref_InitInfo (*deserialize)(agent_connector_kref_InitInfo_$serializer thiz, agent_connector_kref_kotlinx_serialization_Decoder decoder);
          void (*serialize)(agent_connector_kref_InitInfo_$serializer thiz, agent_connector_kref_kotlinx_serialization_Encoder encoder, agent_connector_kref_InitInfo value);
        } $serializer;
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_InitInfo_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_InitInfo_Companion thiz);
        } Companion;
      } InitInfo;
      struct {
        agent_connector_KType* (*_type)(void);
        agent_connector_kref_Initialized (*Initialized)(agent_connector_KInt seen1, const char* msg, agent_connector_kref_kotlinx_serialization_SerializationConstructorMarker serializationConstructorMarker);
        agent_connector_kref_Initialized (*Initialized_)(const char* msg);
        const char* (*get_msg)(agent_connector_kref_Initialized thiz);
        const char* (*component1)(agent_connector_kref_Initialized thiz);
        agent_connector_kref_Initialized (*copy)(agent_connector_kref_Initialized thiz, const char* msg);
        agent_connector_KBoolean (*equals)(agent_connector_kref_Initialized thiz, agent_connector_kref_kotlin_Any other);
        agent_connector_KInt (*hashCode)(agent_connector_kref_Initialized thiz);
        const char* (*toString)(agent_connector_kref_Initialized thiz);
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_Initialized_$serializer (*_instance)();
          agent_connector_kref_kotlinx_serialization_SerialDescriptor (*get_descriptor)(agent_connector_kref_Initialized_$serializer thiz);
          agent_connector_kref_kotlin_Array (*childSerializers)(agent_connector_kref_Initialized_$serializer thiz);
          agent_connector_kref_Initialized (*deserialize)(agent_connector_kref_Initialized_$serializer thiz, agent_connector_kref_kotlinx_serialization_Decoder decoder);
          void (*serialize)(agent_connector_kref_Initialized_$serializer thiz, agent_connector_kref_kotlinx_serialization_Encoder encoder, agent_connector_kref_Initialized value);
        } $serializer;
        struct {
          agent_connector_KType* (*_type)(void);
          agent_connector_kref_Initialized_Companion (*_instance)();
          agent_connector_kref_kotlinx_serialization_KSerializer (*serializer)(agent_connector_kref_Initialized_Companion thiz);
        } Companion;
      } Initialized;
      struct {
        agent_connector_kref_com_epam_drill_common_AgentType (*get_AGENT_TYPE)();
        const char* (*get_PLUGIN_ID)();
        agent_connector_kref_com_epam_drill_core_ws_WsSocket (*createWebSocket)(agent_connector_kref_kotlin_Function2 handler);
        void (*init)(const char* agentId, const char* adminAddress, const char* buildVersion, const char* groupId, const char* instanceId);
        void (*initializeAgent)(const char* agentId, const char* adminAddress, const char* buildVersion, const char* groupId, const char* instanceId, void* function);
        void (*sendMessage_)(const char* messageType, const char* destination, const char* content);
        void (*sendPluginMessage_)(const char* pluginId, const char* content);
        void (*connect)(agent_connector_kref_com_epam_drill_core_ws_WsSocket thiz);
      } drill;
    } root;
  } kotlin;
} agent_connector_ExportedSymbols;
extern agent_connector_ExportedSymbols* agent_connector_symbols(void);
#ifdef __cplusplus
}  /* extern "C" */
#endif
#endif  /* KONAN_AGENT_CONNECTOR_H */
